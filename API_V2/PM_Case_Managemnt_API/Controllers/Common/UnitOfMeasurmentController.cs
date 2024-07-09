
using Microsoft.AspNetCore.Mvc;
using PM_Case_Managemnt_Implementation.DTOS.Common;
using PM_Case_Managemnt_Implementation.Services.Common;


namespace PM_Case_Managemnt_API.Controllers.Common
{
    [Route("api/[controller]")]
    [ApiController]
    public class UnitOfMeasurmentController : ControllerBase
    {

        private readonly IUnitOfMeasurmentService _unitOfMeasurmentService;
        public UnitOfMeasurmentController(IUnitOfMeasurmentService unitOfMeasurmentService)
        {

            _unitOfMeasurmentService = unitOfMeasurmentService;

        }



        [HttpPost]

        public IActionResult Create([FromBody] UnitOfMeasurmentDto unitOfMeasurment)
        {
            try
            {


                var response = _unitOfMeasurmentService.CreateUnitOfMeasurment(unitOfMeasurment);


                return Ok(new { response });


            }

            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error : {ex}");
            }
        }
        [HttpGet]

        public async Task<IActionResult> GetUnitOfMeasurment(Guid subOrgId)
        {



            return Ok(await _unitOfMeasurmentService.GetUnitOfMeasurment(subOrgId));
        }

        [HttpGet("unitmeasurmentlist")]

        public async Task<IActionResult> getUnitOfMeasurment(Guid subOrgId)
        {



            return Ok(await _unitOfMeasurmentService.getUnitOfMeasurmentSelectList(subOrgId));
        }

        [HttpPut]



        public IActionResult Update([FromBody] UnitOfMeasurmentDto unitOfMeasurment)
        {
            try
            {


                var response = _unitOfMeasurmentService.UpdateUnitOfMeasurment(unitOfMeasurment);


                return Ok(new { response });


            }

            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error : {ex}");
            }
        }
    }
}
