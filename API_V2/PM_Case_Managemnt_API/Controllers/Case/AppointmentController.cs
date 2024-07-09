using Microsoft.AspNetCore.Mvc;
using PM_Case_Managemnt_Implementation.DTOS.CaseDto;
using PM_Case_Managemnt_Implementation.Services.CaseMGMT.Appointment;

namespace PM_Case_Managemnt_API.Controllers.Case
{
    [Route("api/case")]
    [ApiController]
    public class AppointmentController : ControllerBase
    {
        private IAppointmentService _appointmentService;

        public AppointmentController(IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        [HttpGet("appointment")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                return Ok(await _appointmentService.GetAll());
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpPost("appointment")]
        public async Task<IActionResult> Create(AppointmentPostDto appointmentPostDto)
        {
            try
            {
                await _appointmentService.Add(appointmentPostDto);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error");
            }
        }


    }
}
