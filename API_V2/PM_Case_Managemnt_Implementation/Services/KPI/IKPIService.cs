using PM_Case_Managemnt_Implementation.DTOS.Common;
using PM_Case_Managemnt_Implementation.DTOS.KPI;
using PM_Case_Managemnt_Implementation.Helpers.Response;

namespace PM_Case_Managemnt_Implementation.Services.KPI
{
    public interface IKPIService
    {
        Task<ResponseMessage<int>> AddKPI(KPIPostDto kpiPost);
        Task<ResponseMessage<int>> AddKPIDetail(KPIDetailsPostDto kpiDetailsPost);
        Task<ResponseMessage<int>> AddKPIData(KPIDataPostDto kpiDataPost);
        Task<ResponseMessage<int>> UpdateKPI(KPIGetDto kpiGet);
        Task<ResponseMessage<int>> UpdateKPIDetail(KPIDetailsGetDto kpiDetailsGet);
        Task<ResponseMessage<string>> LoginKpiDataEncoding(string accessCode);
        Task<ResponseMessage<List<KPIGetDto>>> GetKPIs();
        Task<ResponseMessage<KPIGetDto>> GetKPIById(Guid id);


        Task<ResponseMessage<int>> AddKpiGoal(KPIGoalPostDto kpiGoalPost);
        Task<ResponseMessage<List<SelectListDto>>> GetKpiGoalSelectList(Guid subOrgId);
    }
}
