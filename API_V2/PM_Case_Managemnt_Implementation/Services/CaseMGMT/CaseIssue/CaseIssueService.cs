using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PM_Case_Managemnt_Implementation.DTOS.CaseDto;
using PM_Case_Managemnt_Infrustructure.Data;
using PM_Case_Managemnt_Infrustructure.Models.Auth;
using PM_Case_Managemnt_Infrustructure.Models.Case;
using PM_Case_Managemnt_Infrustructure.Models.CaseModel;
using PM_Case_Managemnt_Infrustructure.Models.Common;

namespace PM_Case_Managemnt_Implementation.Services.CaseMGMT
{
    public class CaseIssueService : ICaseIssueService
    {

        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;

        public CaseIssueService(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
        }

        public async Task<List<CaseEncodeGetDto>> GetNotCompletedCases(Guid subOrgId)
        {
            try
            {
                List<CaseEncodeGetDto> cases = await _dbContext.Cases.Where(ca => ca.AffairStatus != AffairStatus.Completed && ca.SubsidiaryOrganizationId == subOrgId).Include(p => p.Employee).Include(p => p.CaseType).Include(p => p.Applicant).Select(st => new CaseEncodeGetDto
                {
                    Id = st.Id,
                    CaseNumber = st.CaseNumber,
                    LetterNumber = st.LetterNumber,
                    LetterSubject = st.LetterSubject,
                    CaseTypeName = st.CaseType.CaseTypeTitle,
                    ApplicantName = st.Applicant.ApplicantName,
                    EmployeeName = st.Employee.FullName,
                    ApplicantPhoneNo = st.Applicant.PhoneNumber,
                    EmployeePhoneNo = st.Employee.PhoneNumber,
                    CreatedAt = st.CreatedAt.ToString(),
                    ToEmployeeId = _dbContext.CaseHistories.Where(x => x.CaseId == st.Id).FirstOrDefault().ToEmployeeId != null ?
                    _dbContext.CaseHistories.Include(x => x.ToEmployee).Where(x => x.CaseId == st.Id).OrderByDescending(x => x.childOrder).FirstOrDefault().ToEmployee.FullName : "Not Assinged"

                }).ToListAsync();

                return cases;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }


        public async Task IssueCase(CaseIssueDto caseAssignDto)
        {
            try
            {
                string userId = _userManager.Users.Where(x => x.EmployeesId == caseAssignDto.AssignedByEmployeeId).FirstOrDefault().Id;
                //Case caseToAssign = await _dbContext.Cases.SingleOrDefaultAsync(el => el.Id.Equals(caseAssignDto.CaseId));
                // CaseHistory caseHistory = await _dbContext.CaseHistories.SingleOrDefaultAsync(el => el.CaseId.Equals(caseAssignDto.CaseId));

                var toEmployee = caseAssignDto.AssignedToEmployeeId == Guid.Empty || caseAssignDto.AssignedToEmployeeId == null ?
             _dbContext.Employees.FirstOrDefault(
                 e =>
                     e.OrganizationalStructureId == caseAssignDto.AssignedToStructureId &&
                     e.Position == Position.Director).Id : caseAssignDto.AssignedToEmployeeId;

                var toEmployeeCC =
                _dbContext.Employees.FirstOrDefault(
                    e =>
                        e.OrganizationalStructureId == caseAssignDto.ForwardedToStructureId &&
                        e.Position == Position.Director).Id;
                //Case currCase = await _dbContext.Cases.SingleOrDefaultAsync(el => el.Id.Equals(caseAssignDto.CaseId));
                //currCase.AffairStatus = AffairStatus.Assigned;

                //_dbContext.Entry(currCase).Property(curr => curr.AffairStatus).IsModified = true;
                //await _dbContext.SaveChangesAsync();
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



            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public async Task<List<CaseEncodeGetDto>> GetAll(Guid? employeeId)
        {
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

                return cases;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public async Task TakeAction(CaseIssueActionDto caseActionDto)
        {

            try
            {
                var issueCase = _dbContext.CaseIssues.Find(caseActionDto.issueCaseId);
                issueCase.IssueStatus = Enum.Parse<IssueStatus>(caseActionDto.action);
                _dbContext.Entry(issueCase).Property(curr => curr.IssueStatus).IsModified = true;
                _dbContext.SaveChanges();
            }



            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

    }
}
