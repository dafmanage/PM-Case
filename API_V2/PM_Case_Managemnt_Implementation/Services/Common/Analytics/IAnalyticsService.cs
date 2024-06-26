using PM_Case_Managemnt_Implementation.DTOS.Common.Analytics;

namespace PM_Case_Managemnt_Implementation.Services.Common.Analytics
{
    public interface IAnalyticsService
    {
        public Task<SubOrgsPlannedandusedBudgetDtos> GetOverallBudget();

    }
}
