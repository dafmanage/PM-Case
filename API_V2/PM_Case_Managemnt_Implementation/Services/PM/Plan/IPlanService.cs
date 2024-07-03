
using PM_Case_Managemnt_Implementation.DTOS.Common;
using PM_Case_Managemnt_Implementation.DTOS.PM;
using PM_Case_Managemnt_Implementation.Helpers;
using PM_Case_Managemnt_Implementation.Helpers.Response;

namespace PM_Case_Managemnt_Implementation.Services.PM.Plan
{
    public interface IPlanService
    {
        public Task<ResponseMessage<int>> CreatePlan(PlanDto plan);

        public Task<ResponseMessage<List<PlanViewDto>>> GetPlans(Guid? programId, Guid SubOrgId);

        public Task<ResponseMessage<List<SelectListDto>>> GetPlansSelectList(Guid ProgramId);

        public Task<ResponseMessage<PlanSingleViewDto>> GetSinglePlan(Guid planId);
        Task<ResponseMessage> UpdatePlan(PlanDto plan);
        Task<ResponseMessage> DeletePlan(Guid planId);
        //public Task<int> UpdatePrograms(Programs Programs);
        //public Task<List<ProgramDto>> GetPrograms();
        //public Task<List<SelectListDto>> GetProgramsSelectList();
    }
}
