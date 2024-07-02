using PM_Case_Managemnt_Implementation.DTOS.CaseDto;
using PM_Case_Managemnt_Implementation.Helpers.Response;
using PM_Case_Managemnt_Infrustructure.Models.CaseModel;

namespace PM_Case_Managemnt_Implementation.Services.CaseMGMT.AppointmentService
{
    public interface IAppointmentService
    {
        public Task<ResponseMessage<Guid>> Add(AppointmentPostDto appointmentPostDto);
        public Task<ResponseMessage<List<Appointement>>> GetAll();
    }
}
