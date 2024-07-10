using Microsoft.EntityFrameworkCore;
using PM_Case_Managemnt_Implementation.DTOS.CaseDto;
using PM_Case_Managemnt_Implementation.Helpers;
using PM_Case_Managemnt_Implementation.Helpers.Logger;
using PM_Case_Managemnt_Infrustructure.Data;
using PM_Case_Managemnt_Implementation.Helpers.Response;
using PM_Case_Managemnt_Infrustructure.Data;
using PM_Case_Managemnt_Infrustructure.Models.CaseModel;
using PM_Case_Managemnt_Infrustructure.Models.Common;
using System.Net;

namespace PM_Case_Managemnt_Implementation.Services.CaseMGMT.AppointmentWithCalender
{
    public class AppointmentWithCalenderService : IAppointmentWithCalenderService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ISMSHelper _smsService;
        private readonly ILoggerManagerService _logger;

        public AppointmentWithCalenderService(ApplicationDbContext dbContext, ISMSHelper sMSHelper, ILoggerManagerService logger)
        {
            _dbContext = dbContext;
            _smsService = sMSHelper;
            _logger = logger;
        }

       public async Task<ResponseMessage<AppointmentGetDto>> Add(AppointmentWithCalenderPostDto appointmentWithCalender)
        {
            var response = new ResponseMessage<AppointmentGetDto>();
            try
            {
                var hist = await _dbContext.CaseHistories.FindAsync(appointmentWithCalender.CaseId);
                if (hist == null)
                {
                    response.Success = false;
                    response.Message = "Case history not found.";
                    response.ErrorCode = HttpStatusCode.NotFound.ToString();
                    return response;
                }

                var appointment = new AppointementWithCalender
                {
                    Id = Guid.NewGuid(),
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = appointmentWithCalender.CreatedBy,
                    AppointementDate = appointmentWithCalender.AppointementDate,
                    CaseId = hist.CaseId,
                    EmployeeId = appointmentWithCalender.EmployeeId,
                    RowStatus = RowStatus.Active,
                    Remark = appointmentWithCalender.Remark,
                    Time = appointmentWithCalender.Time,
                };

                var caseDetails = await _dbContext.Cases
                    .Include(x => x.Applicant)
                    .FirstOrDefaultAsync(x => x.Id == hist.CaseId);
                if (caseDetails == null)
                {
                    response.Success = false;
                    response.Message = "Case not found.";
                    response.ErrorCode = HttpStatusCode.NotFound.ToString();
                    return response;
                }

                string message = $"{caseDetails.Applicant.ApplicantName} ለጉዳይ ቁጥር፡ {caseDetails.CaseNumber}\n በ {appointmentWithCalender.AppointementDate:yyyy-MM-dd} ቀን በ {appointmentWithCalender.Time} ሰዐት በቢሮ ቁጥር፡ - ይገኙ";

                bool isSmssent = await _smsService.UnlimittedMessageSender(caseDetails.Applicant.PhoneNumber, message, appointment.CreatedBy.ToString());
                if (!isSmssent)
                {
                    await _smsService.UnlimittedMessageSender(caseDetails.PhoneNumber2, message, appointment.CreatedBy.ToString());
                }

                await _dbContext.AppointementWithCalender.AddAsync(appointment);
                await _dbContext.SaveChangesAsync();

                var ev = new AppointmentGetDto
                {
                    Id = appointment.Id.ToString(),
                    description = $"Appointment with {caseDetails.Applicant.ApplicantName} at {appointment.Time}\n Affair Number {caseDetails.CaseNumber}",
                    date = appointment.AppointementDate.ToString(),
                    everyYear = false,
                    type = "event",
                    name = "Appointment"
                };

                response.Success = true;
                response.Message = "Operation Successful";
                response.Data = ev;

                _logger.LogCreate("AppointmentWithCalenderService", appointmentWithCalender.CreatedBy.ToString(), "Appointment with calendar added successfully.");
                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = "An error occurred.";
                response.ErrorCode = HttpStatusCode.InternalServerError.ToString();
                response.Data = null;
            }
            return response;
        }

        public async Task<ResponseMessage<List<AppointmentGetDto>>> GetAll(Guid employeeId)
        {
            var response = new ResponseMessage<List<AppointmentGetDto>>();
            try
            {
                List<AppointmentGetDto> events = [];

                var appointements = _dbContext.AppointementWithCalender.Where(x => x.EmployeeId == employeeId).Include(a => a.Case).ToList();
                appointements.ForEach(a =>
                {
                    var ev = new AppointmentGetDto
                    {
                        Id = a.Id.ToString(),
                        description = "Appointment with " + a?.Case?.Applicant?.ApplicantName + " at " + a.Time + "\n Affair Number " + a.Case.CaseNumber,
                        date = a.AppointementDate.ToString(),
                        everyYear = false,
                        type = "event",
                        name = string.IsNullOrEmpty(a.Remark) ? "Appointment " : a.Remark
                    };
                    events.Add(ev);

                });

                if (events.Count == 0)
                {
                    response.Message = "No available Event";
                    response.Success = false;
                    response.ErrorCode = HttpStatusCode.NotFound.ToString();
                    response.Data = null;
                }

                response.Message = "Events fetched Succesfully";
                response.Success = true;
                response.Data = events;

            }
            catch (Exception ex)
            {
                response.Message = "Faced Error";
                response.Success = false;
                response.ErrorCode = HttpStatusCode.InternalServerError.ToString();
                response.Data = null;
            }

            return response;
        }

    }
}
