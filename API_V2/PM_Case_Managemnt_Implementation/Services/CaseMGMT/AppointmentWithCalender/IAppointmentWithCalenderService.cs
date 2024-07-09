using PM_Case_Managemnt_Implementation.DTOS.CaseDto;
using PM_Case_Managemnt_Implementation.Helpers.Response;

namespace PM_Case_Managemnt_Implementation.Services.CaseMGMT.AppointmentWithCalender
{
    public interface IAppointmentWithCalenderService
    {
        public Task<ResponseMessage<AppointmentGetDto>> Add(AppointmentWithCalenderPostDto appointmentWithCalender);
        public Task<ResponseMessage<List<AppointmentGetDto>>> GetAll(Guid employeeId);
    }
}
