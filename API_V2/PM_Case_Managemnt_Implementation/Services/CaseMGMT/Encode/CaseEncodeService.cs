using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using PM_Case_Managemnt_Implementation.DTOS.CaseDto;
using PM_Case_Managemnt_Implementation.DTOS.Common;
using PM_Case_Managemnt_Implementation.Helpers;
using PM_Case_Managemnt_Implementation.Helpers.Logger;
using PM_Case_Managemnt_Implementation.Helpers.Response;
using PM_Case_Managemnt_Implementation.Hubs.EncoderHub;
using PM_Case_Managemnt_Infrustructure.Data;
using PM_Case_Managemnt_Infrustructure.Models.Auth;
using PM_Case_Managemnt_Infrustructure.Models.CaseModel;
using PM_Case_Managemnt_Infrustructure.Models.Common;
using System.Net;

namespace PM_Case_Managemnt_Implementation.Services.CaseService.Encode
{
    public class CaseEncodeService : ICaseEncodeService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private IHubContext<EncoderHub, IEncoderHubInterface> _encoderHub;
        private readonly ILoggerManagerService _logger;

        public CaseEncodeService(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager,
            IHubContext<EncoderHub, IEncoderHubInterface> encoderHub, ILoggerManagerService logger)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _encoderHub = encoderHub;
            _logger = logger;
        }

        public async Task<ResponseMessage<string>> Add(CaseEncodePostDto caseEncodePostDto)
        {
            var response = new ResponseMessage<string>();
            if (caseEncodePostDto.EmployeeId == null && caseEncodePostDto.ApplicantId == null)
            {
                return new ResponseMessage<string>
                {
                    Message = "Please Provide an Applicant ID or Employee ID",
                    Success = false,
                    ErrorCode = HttpStatusCode.NotFound.ToString()
                };
            }

            var newCase = new Case
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.Now,
                RowStatus = RowStatus.Active,
                CreatedBy = caseEncodePostDto.CreatedBy,
                ApplicantId = caseEncodePostDto.ApplicantId,
                EmployeeId = caseEncodePostDto.EmployeeId,
                LetterNumber = caseEncodePostDto.LetterNumber,
                LetterSubject = caseEncodePostDto.LetterSubject,
                CaseTypeId = caseEncodePostDto.CaseTypeId,
                AffairStatus = AffairStatus.Encoded,
                PhoneNumber2 = caseEncodePostDto.PhoneNumber2,
                Representative = caseEncodePostDto.Representative,
                SubsidiaryOrganizationId = caseEncodePostDto.SubsidiaryOrganizationId,
                CaseNumber = (await GetCaseNumber(caseEncodePostDto.SubsidiaryOrganizationId)).Data
            };

            await _dbContext.AddAsync(newCase);
            await _dbContext.SaveChangesAsync();

            var caseType = await _dbContext.CaseTypes.FindAsync(caseEncodePostDto.CaseTypeId);
            if (caseType == null)
            {
                return new ResponseMessage<string>
                {
                    Message = "Case type not found.",
                    Success = false,
                    ErrorCode = HttpStatusCode.NotFound.ToString()
                };
            }

            if (caseType.CaseForm == CaseForm.Inside)
            {
                var emp = await _dbContext.Employees.FindAsync(caseEncodePostDto.EmployeeId);
                if (emp == null)
                {
                    return new ResponseMessage<string>
                    {
                        Message = "Employee not found.",
                        Success = false,
                        ErrorCode = HttpStatusCode.NotFound.ToString()
                    };
                }

                var user = await _userManager.Users.FirstOrDefaultAsync(x => x.EmployeesId == emp.Id);
                if (user == null)
                {
                    return new ResponseMessage<string>
                    {
                        Message = "User not found.",
                        Success = false,
                        ErrorCode = HttpStatusCode.NotFound.ToString()
                    };
                }

                var startupHistory = new CaseHistory
                {
                    Id = Guid.NewGuid(),
                    CreatedAt = DateTime.Now,
                    CreatedBy = Guid.Parse(user.Id),
                    RowStatus = RowStatus.Active,
                    CaseId = newCase.Id,
                    CaseTypeId = newCase.CaseTypeId,
                    AffairHistoryStatus = AffairHistoryStatus.Pend,
                    FromEmployeeId = emp.Id,
                    FromStructureId = emp.OrganizationalStructureId,
                    ReciverType = ReciverType.Orginal,
                    ToStructureId = emp.OrganizationalStructureId,
                    ToEmployeeId = emp.Id,
                    SecreateryNeeded = emp.Id == Guid.Empty
                };

                newCase.AffairStatus = AffairStatus.Assigned;
                _dbContext.CaseHistories.Add(startupHistory);
                _dbContext.Cases.Update(newCase);
                await _dbContext.SaveChangesAsync();

                var assignedCase = await GetAllTransfred(emp.Id);
                await _encoderHub.Clients.Group(emp.Id.ToString()).getNotification(assignedCase.Data, emp.Id.ToString());
            }

            return new ResponseMessage<string>
            {
                Success = true,
                Message = "Operation Successful.",
                Data = newCase.Id.ToString()
            };
        }

        public async Task<ResponseMessage<string>> Update(CaseEncodePostDto caseEncodePostDto)
        {
            var response = new ResponseMessage<string>();

            if (caseEncodePostDto.EmployeeId == null && caseEncodePostDto.ApplicantId == null)
            {
                return new ResponseMessage<string>
                {
                    Message = "Please Provide an Applicant ID or Employee ID",
                    Success = false,
                    ErrorCode = HttpStatusCode.NotFound.ToString()
                };
            }

            var caseToUpdate = await _dbContext.Cases.FindAsync(caseEncodePostDto.CaseID);
            if (caseToUpdate == null)
            {
                return new ResponseMessage<string>
                {
                    Message = "Case not found.",
                    Success = false,
                    ErrorCode = HttpStatusCode.NotFound.ToString()
                };
            }

            caseToUpdate.ApplicantId = caseEncodePostDto.ApplicantId;
            caseToUpdate.LetterNumber = caseEncodePostDto.LetterNumber;
            caseToUpdate.LetterSubject = caseEncodePostDto.LetterSubject;
            caseToUpdate.CaseTypeId = caseEncodePostDto.CaseTypeId;
            caseToUpdate.AffairStatus = AffairStatus.Encoded;
            caseToUpdate.PhoneNumber2 = caseEncodePostDto.PhoneNumber2;
            caseToUpdate.Representative = caseEncodePostDto.Representative;

            _dbContext.Cases.Update(caseToUpdate);
            await _dbContext.SaveChangesAsync();

            _logger.LogUpdate("CaseEncodeService", caseEncodePostDto.CreatedBy.ToString(), "Case Encode updated Successfully");
            return new ResponseMessage<string>
            {
                Message = "Operation Successful",
                Data = caseToUpdate.Id.ToString(),
                Success = true
            };
        }

        public async Task<ResponseMessage<List<CaseEncodeGetDto>>> GetAll(Guid userId)
        {
            var response = new ResponseMessage<List<CaseEncodeGetDto>>();
            try
            {
                List<CaseEncodeGetDto> cases2 = [];
                List<CaseEncodeGetDto> cases =
                    await _dbContext.Cases.Where(ca => ca.CreatedBy.Equals(userId) && ca.AffairStatus.Equals(AffairStatus.Encoded))
                    .Include(p => p.Employee)
                    .Include(p => p.CaseType)
                    .Include(p => p.Applicant)

                    .Select(st => new CaseEncodeGetDto
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
                        ApplicantId = st.ApplicantId.ToString(),
                        CaseTypeId = st.CaseTypeId.ToString(),
                        Representative = st.Representative,
                        CreatedAt = st.CreatedAt.ToString(),


                    }).OrderByDescending(x => x.CreatedAt).ToListAsync();

                foreach (var item in cases)
                {
                    item.Attachments = await _dbContext.CaseAttachments.Where(x => x.CaseId == item.Id).Select(x => new SelectListDto
                    {
                        Name = x.FilePath,
                        Id = x.Id

                    }).ToListAsync();

                    cases2.Add(item);

                }

                response.Message = "Opertaion Successfull";
                response.Success = true;
                response.Data = cases2;
            }
            catch (Exception ex)
            {
                response.Message = $"{ex.Message}";
                response.Data = null;
                response.Success = false;
                response.ErrorCode = HttpStatusCode.InternalServerError.ToString();
            }

            return response;
        }
        public async Task<ResponseMessage<CaseEncodeGetDto>> GetSingleCase(Guid caseId)
        {
            var response = new ResponseMessage<CaseEncodeGetDto>();

            var caseData = await _dbContext.Cases
                .Where(ca => ca.Id == caseId)
                .Include(p => p.Employee)
                .Include(p => p.CaseType)
                .Include(p => p.Applicant)
                .Select(st => new CaseEncodeGetDto
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
                    ApplicantId = st.ApplicantId.ToString(),
                    CaseTypeId = st.CaseTypeId.ToString(),
                    Representative = st.Representative,
                    CreatedAt = st.CreatedAt.ToString(),
                    CreatedBy = st.CreatedBy.ToString(),
                    Attachments = _dbContext.CaseAttachments.Where(x => x.CaseId == st.Id).Select(x => new SelectListDto
                    {
                        Name = x.FilePath,
                        Id = x.Id
                    }).ToList() 
                }).FirstOrDefaultAsync();

            if (caseData == null)
            {
                return new ResponseMessage<CaseEncodeGetDto>
                {
                    Message = "Case not found.",
                    Success = false,
                    ErrorCode = HttpStatusCode.NotFound.ToString()
                };
            }

            return new ResponseMessage<CaseEncodeGetDto>
            {
                Message = "Operation Successful.",
                Success = true,
                Data = caseData
            };
        }
        public async Task<ResponseMessage<string>> GetCaseNumber(Guid subOrgId)
        {
            var response = new ResponseMessage<string>();

            var subOrgName = await _dbContext.SubsidiaryOrganizations
                .Where(x => x.Id == subOrgId)
                .Select(c => c.OrganizationNameEnglish)
                .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(subOrgName))
            {
                return new ResponseMessage<string>
                {
                    Message = "Subsidiary organization not found.",
                    Success = false,
                    ErrorCode = HttpStatusCode.NotFound.ToString()
                };
            }

            string output = string.Concat(subOrgName.Split(' ')
                                        .Where(w => !string.IsNullOrWhiteSpace(w))
                                        .Select(w => char.ToUpper(w[0])));

            var EthYear = EthiopicDateTime.GetEthiopicYear(DateTime.Now.Day, DateTime.Now.Month, DateTime.Now.Year);

            string caseNumberPrefix = $"{output}{EthYear}-";

            var latestCaseNumber = await _dbContext.Cases
                .Where(x => x.SubsidiaryOrganizationId == subOrgId && x.CaseNumber.StartsWith(caseNumberPrefix))
                .OrderByDescending(x => x.CreatedAt)
                .Select(c => c.CaseNumber)
                .FirstOrDefaultAsync();

            int nextCaseNumber = 1;
            if (latestCaseNumber != null)
            {
                if (int.TryParse(latestCaseNumber.Split('-').LastOrDefault(), out int lastNumber))
                {
                    nextCaseNumber = lastNumber + 1;
                }
            }

            string finalCaseNumber = $"{caseNumberPrefix}{nextCaseNumber}";

            return new ResponseMessage<string>
            {
                Message = "Operation Successful.",
                Data = finalCaseNumber,
                Success = true
            };
        }

        public async Task<ResponseMessage<List<CaseEncodeGetDto>>> GetAllTransfred(Guid employeeId)
        {
            var response = new ResponseMessage<List<CaseEncodeGetDto>>();

            var user = await _dbContext.Employees
                .Include(x => x.OrganizationalStructure)
                .FirstOrDefaultAsync(x => x.Id == employeeId);

            if (user == null)
            {
                return new ResponseMessage<List<CaseEncodeGetDto>>
                {
                    Message = "User not found.",
                    Success = false,
                    ErrorCode = HttpStatusCode.NotFound.ToString()
                };
            }

            var query = _dbContext.CaseHistories
                .Include(c => c.Case.CaseType)
                .Include(x => x.Case.Applicant)
                .Include(x => x.FromStructure)
                .Include(x => x.ToEmployee)
                .Include(x => x.ToStructure)
                .AsQueryable();

            if (user.Position == Position.Secertary)
            {
                query = query.Where(x => x.ToStructureId == user.OrganizationalStructureId &&
                                        (x.AffairHistoryStatus == AffairHistoryStatus.Pend || x.AffairHistoryStatus == AffairHistoryStatus.Transfered) &&
                                        !x.IsConfirmedBySeretery);
            }
            else
            {
                query = query.Where(x => x.ToEmployeeId == employeeId &&
                                        (x.AffairHistoryStatus == AffairHistoryStatus.Pend || x.AffairHistoryStatus == AffairHistoryStatus.Transfered));
            }

            var notifications = await query.Select(x => new CaseEncodeGetDto
            {
                Id = x.Id,
                CaseTypeName = x.Case.CaseType.CaseTypeTitle,
                CaseNumber = x.Case.CaseNumber,
                CreatedAt = x.CreatedAt.ToString(),
                ApplicantName = x.Case.Applicant.ApplicantName,
                ApplicantPhoneNo = x.Case.Applicant.PhoneNumber,
                EmployeeName = x.Case.Employee.FullName,
                EmployeePhoneNo = x.Case.Employee.PhoneNumber,
                LetterNumber = x.Case.LetterNumber,
                LetterSubject = x.Case.LetterSubject,
                FromStructure = x.FromStructure.StructureName,
                ToEmployee = x.ToEmployee.FullName,
                ToStructure = x.ToStructure.StructureName,
                FromEmployeeId = x.FromEmployee.FullName,
                ReciverType = x.ReciverType.ToString(),
                SecreateryNeeded = x.SecreateryNeeded,
                IsConfirmedBySeretery = x.IsConfirmedBySeretery,
                Position = user.Position.ToString(),
                AffairHistoryStatus = x.AffairHistoryStatus.ToString(),
            }).OrderByDescending(x => x.CreatedAt).ToListAsync();

            return new ResponseMessage<List<CaseEncodeGetDto>>
            {
                Message = "Operation successful.",
                Data = notifications,
                Success = true
            };
        }

        public async Task<ResponseMessage<List<CaseEncodeGetDto>>> MyCaseList(Guid employeeId)
        {
            var response = new ResponseMessage<List<CaseEncodeGetDto>>();
            var user = await _dbContext.Employees
                .Include(x => x.OrganizationalStructure)
                .FirstOrDefaultAsync(x => x.Id == employeeId);

            if (user == null)
            {
                return new ResponseMessage<List<CaseEncodeGetDto>>
                {
                    Message = "User not found.",
                    Success = false,
                    ErrorCode = HttpStatusCode.NotFound.ToString()
                };
            }

            IQueryable<CaseHistory> query = _dbContext.CaseHistories
                .Include(x => x.Case)
                .Include(x => x.FromEmployee)
                .Include(x => x.FromStructure)
                .Include(x => x.ToEmployee)
                .Include(x => x.ToStructure)
                .OrderByDescending(x => x.CreatedAt);

            if (user.Position == Position.Secertary)
            {
                query = query.Where(x => 
                    (x.ToEmployee.OrganizationalStructureId == user.OrganizationalStructureId &&
                    x.ToEmployee.Position == Position.Director && !x.IsConfirmedBySeretery) ||
                    (x.FromEmployee.OrganizationalStructureId == user.OrganizationalStructureId &&
                    x.FromEmployee.Position == Position.Director &&
                    !x.IsForwardedBySeretery &&
                    !x.IsConfirmedBySeretery && x.SecreateryNeeded) &&
                    x.AffairHistoryStatus != AffairHistoryStatus.Seen);
            }
            else
            {
                query = query.Where(x => x.AffairHistoryStatus != AffairHistoryStatus.Completed &&
                                        x.AffairHistoryStatus != AffairHistoryStatus.Transfered &&
                                        x.AffairHistoryStatus != AffairHistoryStatus.Revert &&
                                        x.ToEmployeeId == employeeId);
            }

            var allAffairHistory = await query.Select(x => new CaseEncodeGetDto
            {
                Id = x.Id,
                CaseTypeName = x.Case.CaseType.CaseTypeTitle,
                CaseNumber = x.Case.CaseNumber,
                CreatedAt = x.Case.CreatedAt.ToString(),
                ApplicantName = x.Case.Applicant.ApplicantName,
                ApplicantPhoneNo = x.Case.Applicant.PhoneNumber,
                EmployeeName = x.Case.Employee.FullName,
                EmployeePhoneNo = x.Case.Employee.PhoneNumber,
                LetterNumber = x.Case.LetterNumber,
                LetterSubject = x.Case.LetterSubject,
                Position = user.Position.ToString(),
                FromStructure = x.FromStructure.StructureName,
                FromEmployeeId = x.FromEmployee.FullName,
                ReciverType = x.ReciverType.ToString(),
                SecreateryNeeded = x.SecreateryNeeded,
                IsConfirmedBySeretery = x.IsConfirmedBySeretery,
                ToEmployee = x.ToEmployee.FullName,
                ToStructure = x.ToStructure.StructureName,
                AffairHistoryStatus = x.AffairHistoryStatus.ToString()
            }).ToListAsync();

            return new ResponseMessage<List<CaseEncodeGetDto>>
            {
                Message = "Operation Successful.",
                Data = allAffairHistory,
                Success = true
            };
        }

        public async Task<ResponseMessage<List<CaseEncodeGetDto>>> SearchCases(string filter, Guid subOrgId)
        {
            var response = new ResponseMessage<List<CaseEncodeGetDto>>();

            var allAffairHistory = await _dbContext.CaseHistories
                .Include(x => x.Case).ThenInclude(x => x.Applicant)
                .Include(x => x.Case).ThenInclude(x => x.Employee)
                .Include(x => x.FromEmployee)
                .Include(x => x.FromStructure)
                .Include(x => x.ToEmployee)
                .Include(x => x.ToStructure)
                .OrderByDescending(x => x.CreatedAt)
                .Where(x => (x.Case.Applicant.ApplicantName.ToLower().Contains(filter.ToLower()) || 
                            x.Case.Applicant.PhoneNumber.Contains(filter) || 
                            x.Case.CaseNumber.ToLower().Equals(filter.ToLower())) && 
                            x.ReciverType == ReciverType.Orginal && 
                            x.Case.SubsidiaryOrganizationId == subOrgId)
                .Select(x => new CaseEncodeGetDto
                {
                    Id = x.CaseId,
                    CaseTypeName = x.Case.CaseType.CaseTypeTitle,
                    CaseNumber = x.Case.CaseNumber,
                    CreatedAt = x.Case.CreatedAt.ToString(),
                    ApplicantName = x.Case.Applicant.ApplicantName,
                    ApplicantPhoneNo = x.Case.Applicant.PhoneNumber,
                    EmployeeName = x.Case.Employee.FullName,
                    EmployeePhoneNo = x.Case.Employee.PhoneNumber,
                    LetterNumber = x.Case.LetterNumber,
                    LetterSubject = x.Case.LetterSubject,
                    Position = "",
                    FromStructure = x.FromStructure.StructureName,
                    FromEmployeeId = x.FromEmployee.FullName,
                    ReciverType = x.ReciverType.ToString(),
                    SecreateryNeeded = x.SecreateryNeeded,
                    IsConfirmedBySeretery = x.IsConfirmedBySeretery,
                    ToEmployee = x.ToEmployee.FullName,
                    ToStructure = x.ToStructure.StructureName,
                    AffairHistoryStatus = x.AffairHistoryStatus.ToString(),
                    ChildOrder = x.childOrder
                })
                .OrderByDescending(x => x.ChildOrder)
                .ToListAsync();

            response.Message = "Operational Successful.";
            response.Data = allAffairHistory;
            response.Success = true;

            return response;
        }

        public async Task<ResponseMessage<List<CaseEncodeGetDto>>> CompletedCases(Guid subOrgId)
        {
            var response = new ResponseMessage<List<CaseEncodeGetDto>>();

            var cases = await _dbContext.Cases
                .Where(ca => ca.SubsidiaryOrganizationId == subOrgId && 
                            ca.AffairStatus == AffairStatus.Completed && 
                            !ca.IsArchived)
                .Include(p => p.Employee)
                .Include(p => p.CaseType)
                .Include(p => p.Applicant)
                .Select(st => new CaseEncodeGetDto
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
                    AffairHistoryStatus = st.AffairStatus.ToString()
                })
                .ToListAsync();

            response.Message = "Operation Successful.";
            response.Data = cases;
            response.Success = true;

            return response;
        }

        public async Task<ResponseMessage<List<CaseEncodeGetDto>>> GetArchivedCases(Guid subOrgId)
        {
            var response = new ResponseMessage<List<CaseEncodeGetDto>>();

            var cases = await _dbContext.Cases
                .Where(ca => ca.SubsidiaryOrganizationId == subOrgId && ca.IsArchived)
                .Include(p => p.Employee)
                .Include(p => p.CaseType)
                .Include(p => p.Applicant)
                .Include(x => x.Folder.Row.Shelf)
                .Include(x => x.CaseAttachments)
                .Select(st => new CaseEncodeGetDto
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
                    AffairHistoryStatus = st.AffairStatus.ToString(),
                    FolderName = st.Folder.FolderName,
                    RowNumber = st.Folder.Row.RowNumber,
                    ShelfNumber = st.Folder.Row.Shelf.ShelfNumber,
                    Attachments = st.CaseAttachments.Select(x => new SelectListDto
                    {
                        Id = x.Id,
                        Name = x.FilePath
                    }).ToList()
                })
                .ToListAsync();

            response.Message = "Operation Successful.";
            response.Data = cases;
            response.Success = true;

            return response;
        }

    }
}




