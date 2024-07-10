using Microsoft.EntityFrameworkCore;
using PM_Case_Managemnt_Implementation.DTOS.CaseDto;
using PM_Case_Managemnt_Implementation.Helpers.Response;
using PM_Case_Managemnt_Infrustructure.Data;
using PM_Case_Managemnt_Infrustructure.Models.CaseModel;
using PM_Case_Managemnt_Infrustructure.Models.Common;
using PM_Case_Managemnt_Implementation.Helpers.Logger;

namespace PM_Case_Managemnt_Implementation.Services.CaseMGMT.Appointment
{
    public class AppointmentService : IAppointmentService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILoggerManagerService _logger;
        
        public AppointmentService(ApplicationDbContext dbContext, ILoggerManagerService logger)
        {
            _dbContext = dbContext;
            _logger = logger;
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
                _logger.LogCreate("AppointmentService", appointmentPostDto.CreatedBy.ToString(), "Appointment added Successfully");
                return response;
                
            } catch (Exception ex)
            {
                response.Success = false;
                response.Message = "Wasnt not able to create appointment";
                response.ErrorCode = HttpStatusCode.InternalServerError.ToString();
                response.Data = default(Guid);
                //_logger.LogException("AppointmentService", appointmentPostDto.CreatedBy.ToString(), $"Error adding appointment: {ex.Message}");
                return response;
            }
        }
        public async Task<ResponseMessage<List<Appointement>>> GetAll()
        {
            var response = new ResponseMessage<List<AppointmentGetDto>>();
            try
            {
                var appointments = await _dbContext.Appointements
                    .Include(appointment => appointment.Employee)
                    .Include(appointment => appointment.Case)
                    .ToListAsync();
        
                if (!appointments.Any())
                {
                    response.Message = "No available appointments";
                    response.Success = false;
                    response.ErrorCode = nameof(HttpStatusCode.NotFound);
                    response.Data = null;
                    return response;
                }
        
                var result = appointments.Select(appointment => new AppointmentGetDto
                {
                    Id = appointment.Id.ToString(),
                    // Populate other properties as needed
                }).ToList();
        
                response.Message = "Appointments fetched successfully";
                response.Success = true;
                response.Data = result;
            }
            catch (Exception ex)
            {
                // Consider logging the exception here
                response.Message = "Error while fetching";
                response.Success = false;
                response.ErrorCode = nameof(HttpStatusCode.InternalServerError);
                response.Data = null;
            }
            return response;
        }

    }
}
