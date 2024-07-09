using Microsoft.AspNetCore.Mvc;
using PM_Case_Managemnt_Implementation.DTOS.Common.Organization;
using PM_Case_Managemnt_Implementation.Services.Common.SubsidiaryOrganization;

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

        public async Task<IActionResult> getSubsidiaryOrganization()
        {
            return Ok(await _subOrganizationService.GetSubsidiaryOrganization());
        }

        [HttpGet("ById")]

        public async Task<IActionResult> getSubsidiaryOrganizationById(Guid subOrgId)
        {
            return Ok(await _subOrganizationService.GetSubsidiaryOrganizationById(subOrgId));
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

        public async Task<IActionResult> GetSubsidiaryOrganizationSelectList()
        {

            return Ok(await _subOrganizationService.GetSubOrgSelectList());
        }

    }
}
