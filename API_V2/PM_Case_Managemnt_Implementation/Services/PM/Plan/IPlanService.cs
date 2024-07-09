
using PM_Case_Managemnt_Implementation.DTOS.Common;
using PM_Case_Managemnt_Implementation.DTOS.PM;
using PM_Case_Managemnt_Implementation.Helpers;
namespace PM_Case_Managemnt_Implementation.Services.PM.Plann
{
    public interface IPlanService
    {
        public Task<int> CreatePlan(PlanDto plan);
        public Task<List<PlanViewDto>> GetPlans(Guid? programId, Guid SubOrgId);
        public Task<List<SelectListDto>> GetPlansSelectList(Guid ProgramId);
        public Task<PlanSingleViewDto> GetSinglePlan(Guid planId);
        Task<ResponseMessage> UpdatePlan(PlanDto plan);
        Task<ResponseMessage> DeletePlan(Guid planId);
    }
}
