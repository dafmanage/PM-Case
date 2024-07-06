
using PM_Case_Managemnt_Implementation.DTOS.Common;
using PM_Case_Managemnt_Implementation.DTOS.PM;
using PM_Case_Managemnt_Implementation.Helpers.Response;
namespace PM_Case_Managemnt_Implementation.Services.PM.Plan
{
    public interface IPlanService
    {
        public Task<int> CreatePlan(PlanDto plan);

        public Task<List<PlanViewDto>> GetPlans(Guid? programId, Guid SubOrgId);

        public Task<List<SelectListDto>> GetPlansSelectList(Guid ProgramId);

        public Task<PlanSingleViewDto> GetSinglePlan(Guid planId);
        Task<ResponseMessage<int>> UpdatePlan(PlanDto plan);
        Task<ResponseMessage<int>> DeletePlan(Guid planId);
        //public Task<int> UpdatePrograms(Programs Programs);
        //public Task<List<ProgramDto>> GetPrograms();
        //public Task<List<SelectListDto>> GetProgramsSelectList();
    }
}
