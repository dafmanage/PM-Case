using Microsoft.AspNetCore.Mvc;
using PM_Case_Managemnt_Implementation.DTOS.PM;
using PM_Case_Managemnt_Implementation.Services.PM.Plann;

namespace PM_Case_Managemnt_API.Controllers.PM
{
    [Route("api/PM/[controller]")]
    [ApiController]
    public class PlanController(IPlanService planService) : ControllerBase
    {

        private readonly IPlanService _planService = planService;

        [HttpPost]
        public IActionResult Create([FromBody] PlanDto plan)
        {
            try
            {
                var response = _planService.CreatePlan(plan);
                return Ok(new { response });

            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error : {ex}");
            }
        }

        [HttpGet]

        public async Task<List<PlanViewDto>> Getplan(Guid? programId, Guid SubOrgId)
        {
            var response = await _planService.GetPlans(programId, SubOrgId);
            return response;
        }

        [HttpGet("getbyplanid")]

        public async Task<PlanSingleViewDto> GetPlan(Guid planId)
        {
            var response = await _planService.GetSinglePlan(planId);

            return response;
        }


        [HttpGet("getByProgramIdSelectList")]

        public async Task<IActionResult> GetBYPromgramIdSelectList(Guid ProgramId)
        {
            try
            {
                return Ok(await _planService.GetPlansSelectList(ProgramId));
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpPut("editPlan")]
        public async Task<IActionResult> UpdatePlan(PlanDto plan)
        {
            return Ok(await _planService.UpdatePlan(plan));
        }

        [HttpDelete("deletePlan")]
        public async Task<IActionResult> DeletePlan(Guid planId)
        {
            return Ok(await _planService.DeletePlan(planId));
        }


    }
}
