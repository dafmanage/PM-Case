using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PM_Case_Managemnt_Implementation.DTOS.CaseDto;
using PM_Case_Managemnt_Implementation.Helpers.Response;
using PM_Case_Managemnt_Infrustructure.Data;
using PM_Case_Managemnt_Infrustructure.Models.Auth;
using PM_Case_Managemnt_Infrustructure.Models.Case;
using PM_Case_Managemnt_Infrustructure.Models.CaseModel;
using PM_Case_Managemnt_Infrustructure.Models.Common;
using System.Net;

namespace PM_Case_Managemnt_Implementation.Services.CaseMGMT
{
    public class CaseIssueService(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager) : ICaseIssueService
    {

        private readonly ApplicationDbContext _dbContext = dbContext;
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        public async Task<ResponseMessage<List<CaseEncodeGetDto>>> GetNotCompletedCases(Guid subOrgId)
        {
            var response = new ResponseMessage<List<CaseEncodeGetDto>>();
            try
            {
                var cases = await _dbContext.Cases
                    .Where(ca => ca.AffairStatus != AffairStatus.Completed && ca.SubsidiaryOrganizationId == subOrgId)
                    .Include(p => p.Employee)
                    .Include(p => p.CaseType)
                    .Include(p => p.Applicant)
                    .Select(st => new
                    {
                        st.Id,
                        st.CaseNumber,
                        st.LetterNumber,
                        st.LetterSubject,
                        CaseTypeName = st.CaseType.CaseTypeTitle,
                        st.Applicant.ApplicantName,
                        EmployeeName = st.Employee.FullName,
                        ApplicantPhoneNo = st.Applicant.PhoneNumber,
                        EmployeePhoneNo = st.Employee.PhoneNumber,
                        st.CreatedAt,
                        LatestHistory = _dbContext.CaseHistories
                            .Where(x => x.CaseId == st.Id)
                            .OrderByDescending(x => x.childOrder)
                            .FirstOrDefault()
                    })
                    .ToListAsync();

                var caseDtos = cases.Select(c => new CaseEncodeGetDto
                {
                    Id = c.Id,
                    CaseNumber = c.CaseNumber,
                    LetterNumber = c.LetterNumber,
                    LetterSubject = c.LetterSubject,
                    CaseTypeName = c.CaseTypeName,
                    ApplicantName = c.ApplicantName,
                    EmployeeName = c.EmployeeName,
                    ApplicantPhoneNo = c.ApplicantPhoneNo,
                    EmployeePhoneNo = c.EmployeePhoneNo,
                    CreatedAt = c.CreatedAt.ToString("yyyy-MM-dd"), // Assuming a specific format is desired
                    ToEmployeeId = c.LatestHistory?.ToEmployeeId != null ? 
                        _dbContext.Employees.Where(e => e.Id == c.LatestHistory.ToEmployeeId).Select(e => e.FullName).FirstOrDefault() : 
                        "Not Assigned"
                }).ToList();

                if (caseDtos.Count == 0)
                {
                    response.Message = "Could not find not completed cases with the given Id.";
                    response.Success = false;
                    response.ErrorCode = HttpStatusCode.NotFound.ToString();
                }
                else
                {
                    response.Message = "Successfully fetched uncompleted cases.";
                    response.Success = true;
                    response.Data = caseDtos;
                }
            }
            catch (Exception ex)
            {
                response.Message = $"Error: {ex.Message}";
                response.Success = false;
                response.ErrorCode = HttpStatusCode.InternalServerError.ToString();
            }

            return response;
        }

        public async Task<ResponseMessage<string>> IssueCase(CaseIssueDto caseAssignDto)
        {
            var response = new ResponseMessage<string>();

            try
            {
                var user = _userManager.Users.Where(x => x.EmployeesId == caseAssignDto.AssignedByEmployeeId).FirstOrDefault();
            
               if (user == null)
                {
                    response.Success = false;
                    response.Message = "Couldnt find target employee";
                    response.Data = null;
                    response.ErrorCode = HttpStatusCode.NotFound.ToString();
                    return response;
                }

                string userId = user.Id;

                var toEmployee = caseAssignDto.AssignedToEmployeeId == Guid.Empty || caseAssignDto.AssignedToEmployeeId == null ?
                _dbContext.Employees.FirstOrDefault(e =>
                    e.OrganizationalStructureId == caseAssignDto.AssignedToStructureId &&
                    e.Position == Position.Director).Id : caseAssignDto.AssignedToEmployeeId;

                var toEmployeeCC_nullable =
                _dbContext.Employees.FirstOrDefault(
                    e =>
                        e.OrganizationalStructureId == caseAssignDto.ForwardedToStructureId &&
                        e.Position == Position.Director);

                if (toEmployeeCC_nullable == null)
                {
                    response.Message = "Could not find employee";
                    response.Success = false;
                    response.Data = null;
                    response.ErrorCode = HttpStatusCode.NotFound.ToString();
                    return response;
                }


                var toEmployeeCC = toEmployeeCC_nullable.Id;

                var issueCase = new CaseIssue
                {
                    Id = Guid.NewGuid(),
                    Remark = caseAssignDto.Remark,
                    CreatedAt = DateTime.Now,
                    CreatedBy = Guid.Parse(userId),
                    RowStatus = RowStatus.Active,
                    CaseId = caseAssignDto.CaseId,
                    AssignedByEmployeeId = caseAssignDto.AssignedByEmployeeId,
                    AssignedToStructureId = caseAssignDto.AssignedToStructureId,
                    AssignedToEmployeeId = toEmployee,
                    ForwardedToEmployeeId = toEmployeeCC

                };

                _dbContext.CaseIssues.Add(issueCase);
                _dbContext.SaveChanges();

                response.Success = true;
                response.Message = "Issued Successfully";
                response.Data = "OK";
            }
            catch (Exception ex)
            {
                response.Message = $"{ex.Message}";
                response.Success = false;
                response.Data = null;
                response.ErrorCode = HttpStatusCode.InternalServerError.ToString();
            }

            return response;
        }


        public async Task<ResponseMessage<List<CaseEncodeGetDto>>> GetAll(Guid? employeeId)
        {
            var response = new ResponseMessage<List<CaseEncodeGetDto>>();
            try
            {
                List<CaseEncodeGetDto> cases = await _dbContext.CaseIssues.
                    Include(x => x.AssignedByEmployee.OrganizationalStructure).
                    Include(x => x.AssignedToEmployee.OrganizationalStructure).
                    Include(x => x.Case.Applicant).
                    Include(x => x.Case.Employee).
                    Where(ca => ca.IssueStatus.Equals(IssueStatus.Assigned) &&
                    (ca.AssignedByEmployeeId == employeeId || ca.AssignedToEmployeeId == employeeId || ca.ForwardedToEmployeeId == employeeId)).Select(st => new CaseEncodeGetDto
                    {
                        Id = st.Id,
                        CaseNumber = st.Case.CaseNumber,
                        LetterNumber = st.Case.LetterNumber,
                        LetterSubject = st.Case.LetterSubject,
                        CaseTypeName = st.Case.CaseType.CaseTypeTitle,
                        ApplicantName = st.Case.Applicant.ApplicantName,
                        EmployeeName = st.Case.Employee.FullName,
                        ApplicantPhoneNo = st.Case.Applicant.PhoneNumber,
                        EmployeePhoneNo = st.Case.Employee.PhoneNumber,
                        Remark = st.Remark,
                        CreatedAt = st.CreatedAt.ToString(),
                        IssueStatus = st.IssueStatus.ToString(),
                        AssignedTo = st.AssignedToEmployee.FullName + " (" + st.AssignedToEmployee.OrganizationalStructure.StructureName + ")",
                        AssignedBy = st.AssignedByEmployee.FullName + " (" + st.AssignedByEmployee.OrganizationalStructure.StructureName + ")",
                        IssueAction = st.AssignedToEmployeeId == employeeId ? true : false,

                    }).ToListAsync();
                if (cases == null)
                {
                    response.Success = false;
                    response.Message = "No such cases";
                    response.ErrorCode = HttpStatusCode.NotFound.ToString();
                    response.Data = null;
                    return response;
                }
                response.Success = true;
                response.Message = "Cases successfully feteched.";
                response.Data = cases;

                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"{ex.Message}";
                response.ErrorCode = HttpStatusCode.InternalServerError.ToString();
                response.Data = null;

                return response;
            }
        }


        public async Task<ResponseMessage<string>> TakeAction(CaseIssueActionDto caseActionDto)
        {
            var response = new ResponseMessage<string>();

            try
            {
                var issueCase = _dbContext.CaseIssues.Find(caseActionDto.issueCaseId);
                if (issueCase == null)
                {
                    response.Message = "Error while fetching.";
                    response.ErrorCode = HttpStatusCode.NotFound.ToString();
                    response.Data = null;
                    response.Success = false;
                    return response;
                }
                issueCase.IssueStatus = Enum.Parse<IssueStatus>(caseActionDto.action);
                _dbContext.Entry(issueCase).Property(curr => curr.IssueStatus).IsModified = true;
                _dbContext.SaveChanges();
                response.Message = "Operation Successfull";
                response.Success = true;
                response.Data = "OK";
                return response;
            }

            catch (Exception ex)
            {
                response.Message = $"{ex.Message}";
                response.ErrorCode = HttpStatusCode.InternalServerError.ToString();
                response.Data = null;
                response.Success = false;

                return response;
            }
        }

    }
}
