using Microsoft.AspNetCore.Mvc;
using PM_Case_Managemnt_Implementation.DTOS.CaseDto;
using PM_Case_Managemnt_Implementation.Services.CaseMGMT.CaseForwardService;

namespace PM_Case_Managemnt_API.Controllers.Case
{
    [Route("api/case")]
    [ApiController]
    public class CaseForwardController : ControllerBase
    {
        private readonly ICaseForwardService _caseForwardService;

        public CaseForwardController(ICaseForwardService caseForwardService)
        {
            _caseForwardService = caseForwardService;
        }

        [HttpPost("forward")]
        public async Task<IActionResult> Create(CaseForwardPostDto caseForwardPostDto)
        {
            try
            {
                await _caseForwardService.AddMany(caseForwardPostDto);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error");
            }
        }
    }
}
