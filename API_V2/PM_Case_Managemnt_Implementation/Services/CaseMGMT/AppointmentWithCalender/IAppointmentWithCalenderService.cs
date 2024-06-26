using PM_Case_Managemnt_Implementation.DTOS.CaseDto;

namespace PM_Case_Managemnt_Implementation.Services.CaseMGMT.AppointmentWithCalenderService
{
    public interface IAppointmentWithCalenderService
    {
        public Task<AppointmentGetDto> Add(AppointmentWithCalenderPostDto appointmentWithCalender);
        public Task<List<AppointmentGetDto>> GetAll(Guid employeeId);
    }
}
