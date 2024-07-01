﻿using System.Net;
using Microsoft.EntityFrameworkCore;
using PM_Case_Managemnt_Implementation.DTOS.CaseDto;

using PM_Case_Managemnt_Implementation.DTOS.CaseDto;
using PM_Case_Managemnt_Implementation.Helpers;
using PM_Case_Managemnt_Infrustructure.Data;
using PM_Case_Managemnt_Implementation.Helpers.Response;
using PM_Case_Managemnt_Infrustructure.Models.CaseModel;
using PM_Case_Managemnt_Infrustructure.Models.Common;

namespace PM_Case_Managemnt_Implementation.Services.CaseMGMT.AppointmentWithCalenderService
{
    public class AppointmentWithCalenderService : IAppointmentWithCalenderService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ISMSHelper _smsService;

        public AppointmentWithCalenderService(ApplicationDbContext dbContext, ISMSHelper sMSHelper)
        {
            _dbContext = dbContext;
            _smsService = sMSHelper;
        }

       public async Task<ResponseMessage<AppointmentGetDto>> Add(AppointmentWithCalenderPostDto appointmentWithCalender)
        {
            var response  = new ResponseMessage<AppointmentGetDto>();
            try
            {
                //DateTime dateTime= DateTime.Now;

                //if (!string.IsNullOrEmpty(appointmentWithCalender.AppointementDate))
                //{
                //    string[] startDate = appointmentWithCalender.AppointementDate.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                //    dateTime = XAPI.EthiopicDateTime.GetGregorianDate(Int32.Parse(startDate[0]), Int32.Parse(startDate[1]), Int32.Parse(startDate[2]));
                //}
                var hist = _dbContext.CaseHistories.Find(appointmentWithCalender.CaseId);



                AppointementWithCalender appointment = new()
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

                Case cases = _dbContext.Cases.Include(x => x.Applicant).Where(x => x.Id == hist.CaseId).FirstOrDefault();

                string message = cases.Applicant.ApplicantName + " ለጉዳይ ቁጥር፡ " + cases.CaseNumber + "\n በ " + appointmentWithCalender.AppointementDate + " ቀን በ " + appointmentWithCalender.Time +
                 " ሰዐት በቢሮ ቁጥር፡ - ይገኙ";


               bool isSmssent= await _smsService.UnlimittedMessageSender(cases.Applicant.PhoneNumber, message, appointment.CreatedBy.ToString());

                if (!isSmssent)
                    await _smsService.UnlimittedMessageSender(cases.PhoneNumber2, message, appointment.CreatedBy.ToString());

                await _dbContext.AppointementWithCalender.AddAsync(appointment);
                await _dbContext.SaveChangesAsync();


                AppointmentGetDto ev = new AppointmentGetDto();
                ev.id = appointment.Id.ToString();
                ev.description = "Appointment with " + cases.Applicant.ApplicantName + " at " + appointment.Time + "\n Affair Number " + cases.CaseNumber;
                ev.date = appointment.AppointementDate.ToString();
                ev.everyYear = false;
                ev.type = "event";
                ev.name = "Appointment " ;


                response.Success = true;
                response.Message = "Operation Successfull";
                response.Data = ev;


                return response;
            }
            catch (Exception ex)
            {
                response.Message = "Error faced !!";
                response.Data = null;
                response.ErrorCode = HttpStatusCode.InternalServerError.ToString();
                response.Success = false;
                return response;
            }
        }

         public async Task<ResponseMessage<List<AppointmentGetDto>>> GetAll(Guid employeeId)
        {
            var response = new ResponseMessage<List<AppointmentGetDto>>();
            try
            {

                List<AppointmentGetDto> Events = new List<AppointmentGetDto>();
              
                var appointements = _dbContext.AppointementWithCalender.Where(x => x.EmployeeId == employeeId).Include(a => a.Case).ToList();
                appointements.ForEach(a =>
                {
                    var ev = new AppointmentGetDto();
                    ev.id =a.Id.ToString();
                    ev.description = "Appointment with " + a?.Case?.Applicant?.ApplicantName + " at " + a.Time + "\n Affair Number " + a.Case.CaseNumber;
                    ev.date = a.AppointementDate.ToString();
                    ev.everyYear = false;
                    ev.type = "event";
                    ev.name = string.IsNullOrEmpty(a.Remark) ? "Appointment " : a.Remark;
                    Events.Add(ev);
                });

                if (Events == null){
                    response.Message = "No available Event";
                    response.Success = false;
                    response.ErrorCode = HttpStatusCode.NotFound.ToString();
                    response.Data = null;
                    return response;
                }
                response.Message = "Events fetched Succesfully";
                response.Success = true;
                response.Data = Events;
                return response;





               
            } catch(Exception ex) {
                response.Message = "Faced Error";
                response.Success = false;
                response.ErrorCode = HttpStatusCode.InternalServerError.ToString();
                response.Data = null;
                return response;
            }
        }

    }
}
