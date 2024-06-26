using Microsoft.AspNetCore.Mvc;
using PM_Case_Managemnt_Implementation.DTOS.Common.Analytics;
using PM_Case_Managemnt_Implementation.Services.Common.Analytics;

namespace PM_Case_Managemnt_API.Controllers.Common.Analytics
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnalyticsController : ControllerBase
    {
        private readonly IAnalyticsService analyticsService;

        public AnalyticsController(IAnalyticsService analyticsService)
        {
            this.analyticsService = analyticsService;
        }

        [HttpGet]
        [Route("getOverallBudget")]
        public async Task<SubOrgsPlannedandusedBudgetDtos> GetOverallBudget()
        {
            return await analyticsService.GetOverallBudget();
        }
    }
}
