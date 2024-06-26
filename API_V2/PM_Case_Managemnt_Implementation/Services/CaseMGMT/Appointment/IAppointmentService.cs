using PM_Case_Managemnt_Implementation.DTOS.CaseDto;
using PM_Case_Managemnt_Infrustructure.Models.CaseModel;

namespace PM_Case_Managemnt_Implementation.Services.CaseMGMT.AppointmentService
{
    public interface IAppointmentService
    {
        public Task Add(AppointmentPostDto appointmentPostDto);
        public Task<List<Appointement>> GetAll();
    }
}
