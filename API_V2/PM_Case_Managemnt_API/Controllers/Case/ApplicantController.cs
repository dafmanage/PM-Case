using Microsoft.AspNetCore.Mvc;
using PM_Case_Managemnt_Implementation.Services.CaseMGMT.Applicants;
using PM_Case_Managemnt_Implementation.DTOS.CaseDto;

namespace PM_Case_Managemnt_API.Controllers.Case
{
    [Route("api/case")]
    [ApiController]
    public class ApplicantController : ControllerBase
    {

        private readonly IApplicantService _applicantService;

        public ApplicantController(IApplicantService applicantService)
        {
            _applicantService = applicantService;
        }

        [HttpGet("applicant")]
        public async Task<IActionResult> GetAll(Guid subOrgId)
        {
            try
            {
                return Ok(await _applicantService.GetAll(subOrgId));
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpGet("applicantSelectList")]
        public async Task<IActionResult> GetSelectAll(Guid subOrgId)
        {
            try
            {
                return Ok(await _applicantService.GetSelectList(subOrgId));
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error");
            }
        }


        [HttpPost("applicant")]
        public async Task<IActionResult> Create(ApplicantPostDto applicantPostDto)
        {
            try
            {
                var result = await _applicantService.Add(applicantPostDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpPut("applicant")]
        public async Task<IActionResult> Update(ApplicantPostDto applicantPostDto)
        {
            try
            {
                var result = await _applicantService.Update(applicantPostDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpGet("getApplicantById")]

        public async Task<ActionResult> GetApplicantById(Guid applicantId)
        {
            try
            {
                var result = await _applicantService.GetApplicantById(applicantId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error");
            }

        }


    }
}
