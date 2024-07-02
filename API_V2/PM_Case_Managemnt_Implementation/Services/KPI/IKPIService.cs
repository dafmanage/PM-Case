using PM_Case_Managemnt_Implementation.DTOS.Common;
using PM_Case_Managemnt_Implementation.DTOS.KPI;
using PM_Case_Managemnt_Implementation.Helpers;
using PM_Case_Managemnt_Implementation.Helpers.Response;

namespace PM_Case_Managemnt_Implementation.Services.KPI
{
    public interface IKPIService
    {
        Task<ResponseMessage> AddKPI(KPIPostDto kpiPost);
        Task<ResponseMessage> AddKPIDetail(KPIDetailsPostDto kpiDetailsPost);
        Task<ResponseMessage> AddKPIData(KPIDataPostDto kpiDataPost);
        Task<ResponseMessage<List<KPIGetDto>>> GetKPIs();
        Task<ResponseMessage<KPIGetDto>> GetKPIById(Guid id);
        Task<ResponseMessage> UpdateKPI(KPIGetDto kpiGet);
        Task<ResponseMessage> UpdateKPIDetail(KPIDetailsGetDto kpiDetailsGet);
        Task<ResponseMessage<string>> LoginKpiDataEncoding(string accessCode);

        Task<ResponseMessage> AddKpiGoal(KPIGoalPostDto kpiGoalPost);
        Task<ResponseMessage<List<SelectListDto>>> GetKpiGoalSelectList(Guid subOrgId);
    }
}
