using Microsoft.AspNetCore.Mvc;
using PM_Case_Managemnt_Implementation.DTOS.CaseDto;
using PM_Case_Managemnt_Implementation.Services.CaseService.CaseTypes;

namespace PM_Case_Managemnt_API.Controllers.Case
{
    [Route("api/case")]
    [ApiController]
    public class CaseTypeController : ControllerBase
    {
        private readonly ICaseTypeService _caseTypeService;

        public CaseTypeController(ICaseTypeService caseTypeService)
        {
            _caseTypeService = caseTypeService;
        }

        [HttpGet("type")]
        public async Task<IActionResult> GetAll(Guid subOrgId)
        {
            try
            {
                return Ok(await _caseTypeService.GetAll(subOrgId));
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpPost("type")]
        public async Task<IActionResult> Create(CaseTypePostDto caseType)
        {
            try
            {
                await _caseTypeService.Add(caseType);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error");
            }
        }


        [HttpPost("UpdateCaseType")]
        public async Task<IActionResult> UpdateCaseType(CaseTypePostDto caseType)
        {
            try
            {
                await _caseTypeService.UpdateCaseType(caseType);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error");
            }
        }


        [HttpDelete("DeleteCaseType")]
        public async Task<IActionResult> DeleteCaseType(Guid caseTypeId)
        {
            try
            {
                await _caseTypeService.DeleteCaseType(caseTypeId);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error");
            }
        }


        [HttpGet("typeSelectList")]
        public async Task<IActionResult> GetSelectList(Guid subOrgId)
        {
            try
            {

                return Ok(await _caseTypeService.GetAllSelectList(subOrgId));
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpGet("byCaseForm")]
        public async Task<IActionResult> GetALlBYCaseForm(string caseForm, Guid subOrgId)
        {
            try
            {

                return Ok(await _caseTypeService.GetAllByCaseForm(caseForm, subOrgId));
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error");
            }
        }
        [HttpGet("fileSettingsByCaseTypeId")]

        public async Task<IActionResult> GetFileSettingsByCaseTypeId(Guid CaseTypeId)
        {


            try
            {
                return Ok(await _caseTypeService.GetFileSettigs(CaseTypeId));
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error");
            }

        }


        [HttpGet("childCasesByCaseTypeId")]

        public async Task<IActionResult> GetChildCasesByCaseTypeId(Guid CaseTypeId)
        {


            try
            {
                return Ok(await _caseTypeService.GetChildCases(CaseTypeId));
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error");
            }

        }


        [HttpGet("GetChildOrder")]

        public async Task<IActionResult> GetChildOrder(Guid caseTypeId)
        {

            try
            {
                return Ok(_caseTypeService.GetChildOrder(caseTypeId));
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error");
            }

        }

        [HttpGet("GetCaseTypeChildren")]

        public async Task<IActionResult> GetCaseTypeChildren(Guid caseTypeId)
        {

            try
            {
                return Ok(await _caseTypeService.GetCaseTypeChildren(caseTypeId));
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error");
            }

        }

    }
}
