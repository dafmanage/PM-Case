using Microsoft.EntityFrameworkCore;
using PM_Case_Managemnt_Implementation.DTOS.CaseDto;
using PM_Case_Managemnt_Infrustructure.Data;
using PM_Case_Managemnt_Infrustructure.Models.CaseModel;
using PM_Case_Managemnt_Infrustructure.Models.Common;

namespace PM_Case_Managemnt_Implementation.Services.CaseMGMT.History
{
    public class CaseHistoryService : ICaseHistoryService
    {
        private readonly ApplicationDbContext _dbContext;

        public CaseHistoryService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Add(CaseHistoryPostDto caseHistoryPostDto)
        {
            try
            {
                Case currCase = await _dbContext.Cases.SingleOrDefaultAsync(el => el.Id.Equals(caseHistoryPostDto.CaseId));


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

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task SetCaseSeen(CaseHistorySeenDto seenDto)
        {
            try
            {
                CaseHistory history = await CheckHistoryOwner(seenDto.CaseId, seenDto.SeenBy);

                history.SeenDateTime = DateTime.UtcNow;

                _dbContext.Entry(history).Property(history => history.SeenDateTime).IsModified = true;

                await _dbContext.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task CompleteCase(CaseHistoryCompleteDto completeDto)
        {
            try
            {
                CaseHistory history = await CheckHistoryOwner(completeDto.CaseId, completeDto.CompleatedBy);
                history.CompletedDateTime = DateTime.UtcNow;
                history.AffairHistoryStatus = AffairHistoryStatus.Completed;

                _dbContext.Entry(history).Property(history => history.CompletedDateTime).IsModified = true;

                Case currCase = await _dbContext.Cases.FindAsync(completeDto.CaseId);

                if (currCase == null)
                    throw new Exception("No Case with the given Id.");
                currCase.AffairStatus = AffairStatus.Completed;
                currCase.CompletedAt = DateTime.Now;

                _dbContext.Entry(currCase).Property(history => history.AffairStatus).IsModified = true;
                _dbContext.Entry(currCase).Property(history => history.CompletedAt).IsModified = true;

                await _dbContext.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private async Task<CaseHistory> CheckHistoryOwner(Guid CaseId, Guid EmpId)
        {
            try
            {
                CaseHistory history = await _dbContext.CaseHistories.SingleOrDefaultAsync(history => history.CaseId.Equals(CaseId));

                if (history == null)
                    throw new Exception("No history found for the given Case.");

                if (EmpId != history.ToEmployeeId)
                    throw new Exception("Error! You can only alter Cases addressed to you.");

                return history;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public async Task<List<CaseEncodeGetDto>> GetCaseHistory(Guid EmployeeId, Guid CaseHistoryId)
        {
            Employee user = _dbContext.Employees.Include(x => x.OrganizationalStructure).Where(x => x.Id == EmployeeId).FirstOrDefault();


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


            return affairHistories;
        }
    }
}
