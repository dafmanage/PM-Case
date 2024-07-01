using System.Net;
using Microsoft.EntityFrameworkCore;
using PM_Case_Managemnt_Implementation.DTOS.CaseDto;
using PM_Case_Managemnt_Implementation.Helpers.Response;
using PM_Case_Managemnt_Infrustructure.Data;
using PM_Case_Managemnt_Infrustructure.Models.CaseModel;
using PM_Case_Managemnt_Infrustructure.Models.Common;

namespace PM_Case_Managemnt_Implementation.Services.CaseMGMT.AppointmentService
{
    public class AppointmentService : IAppointmentService
    {
        private readonly ApplicationDbContext _dbContext;

        public AppointmentService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }


        public async Task<ResponseMessage<Guid>> Add(AppointmentPostDto appointmentPostDto)
        {

            var response = new ResponseMessage<Guid>();
        
            try
            {
                
            
                Appointement appointment = new()
                {
                    Id = Guid.NewGuid(),
                    CreatedAt = DateTime.Now,
                    CreatedBy = appointmentPostDto.CreatedBy,
                    CaseId = appointmentPostDto.CaseId,
                    EmployeeId = appointmentPostDto.EmployeeId,
                    IsSmsSent = false,
                    Remark = appointmentPostDto.Remark,
                    RowStatus = RowStatus.Active
                };

                await _dbContext.Appointements.AddAsync(appointment);
                await _dbContext.SaveChangesAsync();

                response.Success = true;
                response.Message = "Appointment added Succesfully";
                response.Data = appointment.Id;

                return response;
                
            } catch (Exception ex)
            {
                response.Success = false;
                response.Message = "Wasnt not able to create appointment";
                response.ErrorCode = HttpStatusCode.InternalServerError.ToString();
                response.Data = default(Guid);
                return response;
            }
        }
        public async Task<ResponseMessage<List<Appointement>>> GetAll()
        {
            var response = new ResponseMessage<List<Appointement>>();
            try
            {
                List<Appointement> appointments = await _dbContext.Appointements.Include(appointment => appointment.Employee).Include(appointment => appointment.Case).ToListAsync();
                //List<AppointmentGetDto> result = new();
                if (appointments == null){
                    response.Message = "No available appointment";
                    response.Success = false;
                    response.ErrorCode = HttpStatusCode.NotFound.ToString();
                    response.Data = null;
                    return response;
                }
                /*
                foreach (Appointement appointement in appointments)
                {
                    result.Add(new AppointmentGetDto()
                    {
                        id = appointement.Id.ToString(),
                        
                    });
                }
                */
                response.Message = "Appointments fetched Succesfully";
                response.Success = true;
                response.Data = appointments;
                return response;
            } catch (Exception ex)
            {
                response.Message = "Error while fetching";
                response.Success = false;
                response.ErrorCode = HttpStatusCode.InternalServerError.ToString();
                response.Data = null;
                return response;
            }
        }

    }
}
