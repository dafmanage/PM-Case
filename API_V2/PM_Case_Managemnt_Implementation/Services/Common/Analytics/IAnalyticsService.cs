using PM_Case_Managemnt_Implementation.DTOS.Common.Analytics;
using PM_Case_Managemnt_Implementation.Helpers.Response;

namespace PM_Case_Managemnt_Implementation.Services.Common.Analytics
{
    public interface IAnalyticsService
    {
        public Task<ResponseMessage<SubOrgsPlannedandusedBudgetDtos>> GetOverallBudget();

    }
}
