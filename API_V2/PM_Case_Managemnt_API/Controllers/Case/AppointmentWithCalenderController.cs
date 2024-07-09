using Microsoft.AspNetCore.Mvc;
using PM_Case_Managemnt_Implementation.DTOS.CaseDto;
using PM_Case_Managemnt_Implementation.Services.CaseMGMT.AppointmentWithCalender;

namespace PM_Case_Managemnt_API.Controllers.Case
{
    [Route("api/case")]
    [ApiController]
    public class AppointmentWithCalenderController : ControllerBase
    {
        private readonly IAppointmentWithCalenderService _appointmentWithCalenderService;

        public AppointmentWithCalenderController(IAppointmentWithCalenderService appointmentWithCalenderService)
        {
            _appointmentWithCalenderService = appointmentWithCalenderService;
        }

        [HttpGet("appointmetWithCalender")]
        public async Task<IActionResult> GetAll(Guid employeeId)
        {
            try
            {
                return Ok(await _appointmentWithCalenderService.GetAll(employeeId));
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpPost("appointmetWithCalender")]
        public async Task<IActionResult> Create(AppointmentWithCalenderPostDto appointmentWithCalender)
        {
            try
            {

                return Ok(await _appointmentWithCalenderService.Add(appointmentWithCalender));
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error");
            }
        }

    }
}
