using PM_Case_Managemnt_Implementation.DTOS.Case;
using static PM_Case_Managemnt_Implementation.Services.Common.Dashoboard.DashboardService;

namespace PM_Case_Managemnt_Implementation.Services.Common.Dashoboard
{
    public interface IDashboardService
    {

        public Task<DashboardDto> GetPendingCase(Guid subOrgId, string startat, string endat);
        public Task<barChartDto> GetMonthlyReport(Guid subOrgId, int year);

        public Task<PMDashboardDto> GetPMDashboardDto(Guid empID, Guid subOrgId);

        public Task<PmDashboardBarchartDto> BudgetYearVsContribution(Guid empID, Guid subOrgId);


    }
}
