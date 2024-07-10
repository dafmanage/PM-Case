using PM_Case_Managemnt_Implementation.DTOS.CaseDto;
using PM_Case_Managemnt_Infrustructure.Data;
using PM_Case_Managemnt_Infrustructure.Models.CaseModel;

namespace PM_Case_Managemnt_Implementation.Services.CaseMGMT.CaseForwardService
{
    public class CaseForwardService(ApplicationDbContext dbContext) : ICaseForwardService
    {
        private readonly ApplicationDbContext _dbContext = dbContext;

        public async Task AddMany(CaseForwardPostDto caseForwardPostDto)
        {
            try
            {
                List<CaseForward> caseForwards = [];
                //TODO: Implement method

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
