using Microsoft.AspNetCore.Mvc;
using PM_Case_Managemnt_Implementation.DTOS.Common;
using PM_Case_Managemnt_Implementation.DTOS.Common.Organization;
using PM_Case_Managemnt_Implementation.Services.Common.SubsidiaryOrganization;
using PM_Case_Managemnt_Infrustructure.Models.Common.Organization;

namespace PM_Case_Managemnt_API.Controllers.Common.Organization
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubOrganizationController : ControllerBase
    {
        private readonly ISubsidiaryOrganizationService _subOrganizationService;

        public SubOrganizationController(ISubsidiaryOrganizationService subOrganzationService)
        {
            _subOrganizationService = subOrganzationService;
        }

        [HttpPost]

        public IActionResult Create([FromBody] SubOrgDto subOrg)
        {
            try
            {
                var response = _subOrganizationService.CreateSubsidiaryOrganization(subOrg);
                return Ok(new { response });
            }

            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error : {ex}");
            }
        }

        [HttpGet]

        public async Task<List<SubsidiaryOrganization>> getSubsidiaryOrganization()
        {
            var result = await _subOrganizationService.GetSubsidiaryOrganization();
            return result.Data;
        }

        [HttpGet("ById")]

        public async Task<SubsidiaryOrganization> getSubsidiaryOrganizationById(Guid subOrgId)
        {
            var result = await _subOrganizationService.GetSubsidiaryOrganizationById(subOrgId);
            return result.Data;
        }

        [HttpPut, DisableRequestSizeLimit]
        public async Task<IActionResult> Update(SubOrgDto subOrg)
        {
            try
            {
                return Ok(await _subOrganizationService.UpdateSubsidiaryOrganization(subOrg));
            }

            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error : {ex}");
            }
        }

        [HttpDelete, DisableRequestSizeLimit]
        public async Task<IActionResult> Delete(Guid suborgId)
        {
            try
            {
                return Ok(await _subOrganizationService.DeleteSubsidiaryOrganization(suborgId));
            }

            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error : {ex}");
            }
        }

        [HttpGet("selectlist")]

        public async Task<List<SelectListDto>> GetSubsidiaryOrganizationSelectList()
        {

            var result = await _subOrganizationService.GetSubOrgSelectList();
            return result.Data;
        }

    }
}
