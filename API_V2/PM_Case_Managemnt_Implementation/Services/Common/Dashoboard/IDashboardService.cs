using PM_Case_Managemnt_Implementation.DTOS.Case;
using PM_Case_Managemnt_Implementation.Helpers.Response;
using static PM_Case_Managemnt_Implementation.Services.Common.Dashoboard.DashboardService;

namespace PM_Case_Managemnt_Implementation.Services.Common.Dashoboard
{
    public interface IDashboardService
    {

        public  Task<ResponseMessage<DashboardDto>> GetPendingCase(Guid subOrgId, string startat, string endat);
        public Task<ResponseMessage<barChartDto>> GetMonthlyReport(Guid subOrgId, int year);

        public Task<ResponseMessage<PMDashboardDto>> GetPMDashboardDto(Guid empID, Guid subOrgId);

        public Task<ResponseMessage<PmDashboardBarchartDto>> BudgetYearVsContribution(Guid empID, Guid subOrgId);


    }
}
