using PM_Case_Managemnt_Implementation.DTOS.CaseDto;
using PM_Case_Managemnt_Infrustructure.Data;
using PM_Case_Managemnt_Infrustructure.Models.CaseModel;

namespace PM_Case_Managemnt_Implementation.Services.CaseMGMT.CaseForwardService
{
    public class CaseForwardService : ICaseForwardService
    {
        private readonly ApplicationDbContext _dbContext;

        public CaseForwardService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }


        public async Task AddMany(CaseForwardPostDto caseForwardPostDto)
        {
            try
            {
                List<CaseForward> caseForwards = [];

                //foreach(Guid forwardToStructureId in caseForwardPostDto.ForwardedToStructureId)
                //{
                //    caseForwards.Add(
                //    new()
                //    {
                //        Id = Guid.NewGuid(),
                //        CreatedAt = DateTime.Now,
                //        CreatedBy = caseForwardPostDto.CreatedBy,
                //        CaseId = caseForwardPostDto.CaseId,
                //        ForwardedByEmployeeId = caseForwardPostDto.ForwardedByEmployeeId,
                //        ForwardedToStructureId = forwardToStructureId,
                //        RowStatus = RowStatus.Active,
                //    }
                //        );
                //}

                //await _dbContext.CaseForwards.AddRangeAsync(caseForwards);
                //await _dbContext.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
