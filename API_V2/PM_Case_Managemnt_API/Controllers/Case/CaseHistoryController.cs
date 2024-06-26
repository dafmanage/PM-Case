using Microsoft.AspNetCore.Mvc;
using PM_Case_Managemnt_Implementation.DTOS.CaseDto;
using PM_Case_Managemnt_Implementation.Services.CaseMGMT.History;

namespace PM_Case_Managemnt_API.Controllers.Case
{
    [Route("api/case")]
    [ApiController]
    public class CaseHistoryController : ControllerBase
    {
        private readonly ICaseHistoryService _caseHistoryService;

        public CaseHistoryController(ICaseHistoryService caseHistoryService)
        {
            _caseHistoryService = caseHistoryService;
        }

        [HttpPost("setSeen")]
        public async Task<IActionResult> SetSeen(CaseHistorySeenDto seenDto)
        {
            try
            {
                await _caseHistoryService.SetCaseSeen(seenDto);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpPost("setComplete")]
        public async Task<IActionResult> SetComplete(CaseHistorySeenDto seenDto)
        {
            try
            {
                await _caseHistoryService.SetCaseSeen(seenDto);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpGet("getHistories")]
        public async Task<IActionResult> GetHistories(Guid EmployeeId, Guid CaseHistoryId)
        {
            try
            {
                return Ok(await _caseHistoryService.GetCaseHistory(EmployeeId, CaseHistoryId));

            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error");
            }

        }



    }
}
