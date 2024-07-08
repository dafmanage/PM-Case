using System.Net;
using Microsoft.EntityFrameworkCore;
using PM_Case_Managemnt_Implementation.DTOS.CaseDto;
using PM_Case_Managemnt_Implementation.Helpers.Logger;
using PM_Case_Managemnt_Implementation.Helpers.Response;
using PM_Case_Managemnt_Infrustructure.Data;
using PM_Case_Managemnt_Infrustructure.Models.CaseModel;
using PM_Case_Managemnt_Infrustructure.Models.Common;

namespace PM_Case_Managemnt_Implementation.Services.CaseMGMT.History
{
    public class CaseHistoryService : ICaseHistoryService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILoggerManagerService _logger;

        public CaseHistoryService(ApplicationDbContext dbContext, ILoggerManagerService logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<ResponseMessage<int>> Add(CaseHistoryPostDto caseHistoryPostDto)
        {
            var response = new ResponseMessage<int>();
            try
            {
                Case? currCase = await _dbContext.Cases.SingleOrDefaultAsync(el => el.Id.Equals(caseHistoryPostDto.CaseId));

                if (currCase == null){

                    response.Message = "Case Not found";
                    response.ErrorCode = HttpStatusCode.NotFound.ToString();
                    response.Success = false;
                    response.Data = 0;

                    return response;
                }

                if (currCase == null)
                    throw new Exception("Case Not found");


                CaseHistory history = new()
                {
                    Id = Guid.NewGuid(),
                    CreatedAt = DateTime.Now,
                    CreatedBy = caseHistoryPostDto.CreatedBy,
                    RowStatus = RowStatus.Active,
                    CaseId = caseHistoryPostDto.CaseId,
                    CaseTypeId = caseHistoryPostDto.CaseTypeId,
                    FromEmployeeId = caseHistoryPostDto.FromEmployeeId,
                    ToEmployeeId = caseHistoryPostDto.ToEmployeeId,
                    FromStructureId = caseHistoryPostDto.FromStructureId,
                    ToStructureId = caseHistoryPostDto.ToStructureId,
                    AffairHistoryStatus = AffairHistoryStatus.Waiting,
                    //SeenDateTime = caseHistoryPostDto?.SeenDateTime,
                    //TransferedDateTime = caseHistoryPostDto?.TransferedDateTime,
                    //CompletedDateTime = caseHistoryPostDto?.CompletedDateTime,
                    //RevertedAt = caseHistoryPostDto?.RevertedAt,
                    IsSmsSent = caseHistoryPostDto.IsSmsSent,
                    //IsConfirmedBySeretery = caseHistoryPostDto.IsConfirmedBySeretery,
                    //IsForwardedBySeretery = caseHistoryPostDto.IsConfirmedBySeretery,
                    //SecreteryConfirmationDateTime = caseHistoryPostDto?.SecreteryConfirmationDateTime,
                    SecreteryId = caseHistoryPostDto?.SecreteryId,
                    //ForwardedDateTime = caseHistoryPostDto?.ForwardedDateTime,
                    //ForwardedById = caseHistoryPostDto?.ForwardedById,
                };

                if (history.ToEmployeeId == currCase.EmployeeId)
                    history.ReciverType = ReciverType.Orginal;
                else
                    history.ReciverType = ReciverType.Cc;

                await _dbContext.CaseHistories.AddAsync(history);
                await _dbContext.SaveChangesAsync();
                response.Message = "Operation Successfull";
                response.Success = true;
                response.Data = 1;
                _logger.LogCreate("CaseHistoryService", caseHistoryPostDto.CreatedBy.ToString(), "Case History added Successfully");
                return response;

            }
            catch (Exception ex)
            {
                response.Message = $"{ex.Message}";
                response.ErrorCode = HttpStatusCode.InternalServerError.ToString();
                response.Success = false;
                response.Data = 0;

                return response;
            }
        }

        public async Task<ResponseMessage<int>> SetCaseSeen(CaseHistorySeenDto seenDto)
        {
            var response = new ResponseMessage<int>();
            try
            {
                ResponseMessage<CaseHistory> history_checker = await CheckHistoryOwner(seenDto.CaseId, seenDto.SeenBy);
                CaseHistory? history = history_checker.Data;

                if (history == null){
                    response.Message = history_checker.Message;
                    response.Success = false;
                    response.ErrorCode = history_checker.ErrorCode;
                    response.Data = 0;

                    return response;
                }
                
                history.SeenDateTime = DateTime.UtcNow;

                _dbContext.Entry(history).Property(history => history.SeenDateTime).IsModified = true;

                await _dbContext.SaveChangesAsync();
                response.Message = "Operation Successfull.";
                response.Data = 1;
                response.Success = true;
                _logger.LogUpdate("CaseHistoryService", seenDto.CaseId.ToString(), "Case added to seen Successfully");
                return response;
            }
            catch (Exception ex)
            {
                response.Message = $"{ex.Message}";
                response.ErrorCode = HttpStatusCode.InternalServerError.ToString();
                response.Success = false;
                response.Data = 0;

                return response;
            }
        }

        public async Task<ResponseMessage<int>> CompleteCase(CaseHistoryCompleteDto completeDto)
        {
            var response = new ResponseMessage<int>();
            try
            {
                ResponseMessage<CaseHistory> going_to_be_used_below = await CheckHistoryOwner(completeDto.CaseId, completeDto.CompleatedBy);
                CaseHistory? history = going_to_be_used_below.Data;

                if (history == null){
                    response.Message = going_to_be_used_below.Message;
                    response.Success = false;
                    response.ErrorCode = going_to_be_used_below.ErrorCode;
                    response.Data = 0;

                    return response;
                    
                }
                history.CompletedDateTime = DateTime.UtcNow;
                history.AffairHistoryStatus = AffairHistoryStatus.Completed;

                _dbContext.Entry(history).Property(history => history.CompletedDateTime).IsModified = true;

                Case? currCase = await _dbContext.Cases.FindAsync(completeDto.CaseId);
                if (currCase == null){

                    response.Message = "No Case with the given Id.";
                    response.ErrorCode = HttpStatusCode.NotFound.ToString();
                    response.Success = false;
                    response.Data = 0;

                    return response;
                }
                currCase.AffairStatus = AffairStatus.Completed;
                currCase.CompletedAt = DateTime.Now;

                _dbContext.Entry(currCase).Property(history => history.AffairStatus).IsModified = true;
                _dbContext.Entry(currCase).Property(history => history.CompletedAt).IsModified = true;

                await _dbContext.SaveChangesAsync();
                
                response.Message = "Operation Successfull.";
                response.Data = 1;
                response.Success = true;
                _logger.LogUpdate("CaseHistoryService", completeDto.CaseId.ToString(), "Case added to completed Successfully");
                return response;

            }
            catch (Exception ex)
            {
                response.Message = $"{ex.Message}";
                response.ErrorCode = HttpStatusCode.InternalServerError.ToString();
                response.Success = false;
                response.Data = 0;

                return response;
            }
        }

        private async Task<ResponseMessage<CaseHistory>> CheckHistoryOwner(Guid CaseId, Guid EmpId)
        {
            var response = new ResponseMessage<CaseHistory>();
            try
            {
                CaseHistory? history = await _dbContext.CaseHistories.SingleOrDefaultAsync(history => history.CaseId.Equals(CaseId));

                if (history == null){
                    response.Message = "NO history found for the given Case.";
                    response.ErrorCode = HttpStatusCode.NotFound.ToString();
                    response.Success = false;
                    response.Data = null;

                    return response;
                }

                if (EmpId != history.ToEmployeeId)
                {
                    response.Message = "Error! you can only alter Cases addressed to you.";
                    response.ErrorCode = HttpStatusCode.Forbidden.ToString();
                    response.Success = false;
                    response.Data = null;

                    return response;
                }

               
                response.Message = "Operation Successfull";
                response.Data = history;
                response.Success = true;

                return response;
            }
            catch (Exception ex)
            {
                response.Message = $"{ex.Message}";
                response.ErrorCode = HttpStatusCode.InternalServerError.ToString();
                response.Success = false;
                response.Data = null;

                return response;
            }
        }


        public async Task<ResponseMessage<List<CaseEncodeGetDto>>> GetCaseHistory(Guid EmployeeId, Guid CaseHistoryId)
        {
            var response = new ResponseMessage<List<CaseEncodeGetDto>>();
            Employee? user = _dbContext.Employees.Include(x => x.OrganizationalStructure).Where(x => x.Id == EmployeeId).FirstOrDefault();

            if (user == null){
                response.Message = "User Not Found";
                response.Success = false;
                response.Data = null;
                response.ErrorCode = HttpStatusCode.NotFound.ToString();

                return response;
            }

            var caseHistory = _dbContext.CaseHistories.Find(CaseHistoryId);
            var affair = _dbContext.Cases.Include(x => x.CaseType).Where(x => x.Id == caseHistory.CaseId).FirstOrDefault();

            // ViewBag.affairtypes = Db.AffairTypes.Where(x => x.ParentAffairTypeId == af.AffairTypeId).ToList();
            //ViewBag.parentaffairname = af.AffairType.AffairTypeTitle;




            List<CaseEncodeGetDto> affairHistories = await _dbContext.CaseHistories
                .Include(a => a.Case)
                .Include(a => a.FromStructure)
                .Include(a => a.FromEmployee)
                .Include(a => a.ToStructure)
                .Include(a => a.ToEmployee)
                .Include(a => a.CaseType)

                .Where(x => x.CaseId == affair.Id)
                .OrderByDescending(x => x.CreatedAt)
                .ThenBy(x => x.ReciverType).Select(x => new CaseEncodeGetDto
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
                    ToEmployeeId = x.ToEmployeeId.ToString(),
                    ReciverType = x.ReciverType.ToString(),
                    SecreateryNeeded = x.SecreateryNeeded,
                    IsConfirmedBySeretery = x.IsConfirmedBySeretery,
                    ToEmployee = x.ToEmployee.FullName,
                    ToStructure = x.ToStructure.StructureName,
                    IsSMSSent = x.IsSmsSent,
                    AffairHistoryStatus = x.AffairHistoryStatus.ToString()
                }).ToListAsync();


            response.Message = "Operation Successfull.";
            response.Data = affairHistories;
            response.Success = true;
            
            return response;
        }
    }
}
