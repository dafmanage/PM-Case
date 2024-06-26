using Microsoft.AspNetCore.Mvc;
using PM_Case_Managemnt_Implementation.DTOS.CaseDto;
using PM_Case_Managemnt_Implementation.Services.CaseMGMT;

namespace PM_Case_Managemnt_API.Controllers.Case
{
    [Route("api/case/[controller]")]
    [ApiController]
    public class CaseIssueController : ControllerBase
    {

        private readonly ICaseIssueService _caseIssueService;

        public CaseIssueController(ICaseIssueService caseIssueService)
        {
            _caseIssueService = caseIssueService;
        }

        [HttpGet()]
        public async Task<IActionResult> GetAll(Guid? employeeId)
        {
            try
            {
                var results = await _caseIssueService.GetAll(employeeId);
                return Ok(results);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpGet("getNotCompletedCases")]
        public async Task<IActionResult> GetNotCompletedCases(Guid subOrgId)
        {
            try
            {
                var results = await _caseIssueService.GetNotCompletedCases(subOrgId);
                return Ok(results);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CaseIssue(CaseIssueDto caseAssignDto)
        {
            try
            {
                await _caseIssueService.IssueCase(caseAssignDto);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error");
            }
        }


        [HttpPost("takeAction")]
        public async Task<IActionResult> TakeAction(CaseIssueActionDto caseActionDto)
        {

            try
            {
                await _caseIssueService.TakeAction(caseActionDto);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error");
            }

        }
    }
}
