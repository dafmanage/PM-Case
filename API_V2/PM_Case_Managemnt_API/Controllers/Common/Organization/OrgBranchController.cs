
using Microsoft.AspNetCore.Mvc;
using PM_Case_Managemnt_Implementation.DTOS.Common;
using PM_Case_Managemnt_Implementation.Services.Common;

namespace PM_Case_Managemnt_API.Controllers.Common.Organization
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrgBranchController : ControllerBase
    {

        private readonly IOrgBranchService _branchService;
        public OrgBranchController(IOrgBranchService branchService)
        {

            _branchService = branchService;

        }



        [HttpPost]
        public IActionResult Create([FromBody] OrgBranchDto organizationBranch)
        {
            try
            {


                var response = _branchService.CreateOrganizationalBranch(organizationBranch);


                return Ok(new { response });


            }

            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error : {ex}");
            }
        }
        [HttpGet]

        public async Task<List<OrgStructureDto>> GetBranches(Guid SubOrgId)
        {


            var result = await _branchService.GetOrganizationBranches(SubOrgId);
            return result.Data;
        }

        [HttpGet("branchlist")]

        public async Task<List<SelectListDto>> GetBranchList(Guid SubOrgId)
        {


            var result = await _branchService.getBranchSelectList(SubOrgId);
            return result.Data;
        }


        [HttpPut]

        public IActionResult Update([FromBody] OrgBranchDto organizationBranch)
        {
            try
            {


                var response = _branchService.UpdateOrganizationBranch(organizationBranch);


                return Ok(new { response });


            }

            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error : {ex}");
            }
        }




    }
}
