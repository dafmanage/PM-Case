using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PM_Case_Managemnt_Implementation.DTOS;
using PM_Case_Managemnt_Implementation.DTOS.CaseDto;
using PM_Case_Managemnt_Implementation.Helpers;
using PM_Case_Managemnt_Infrustructure.Data;
using PM_Case_Managemnt_Infrustructure.Models.Auth;
using PM_Case_Managemnt_Infrustructure.Models.CaseModel;
using PM_Case_Managemnt_Infrustructure.Models.Common;
using System.Net.Http.Headers;

namespace PM_Case_Managemnt_API.Controllers.Case
{
    [Route("api/MobileUser")]
    [ApiController]
    public class AffairForMobileController : ControllerBase
    {

        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ISMSHelper _smshelper;


        public AffairForMobileController(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager, ISMSHelper sMSHelper)
        {
            _db = dbContext;
            _userManager = userManager;
            _smshelper = sMSHelper;
        }


        [HttpPost]
        [Route("login")]

        public Tbluser login([FromBody] Tbluser user)
        {
            Employee employee = _db.Employees.Include(x => x.OrganizationalStructure).FirstOrDefault(e => e.UserName == user.UserName && e.Password == user.Password);


            Tbluser userView = new Tbluser();
            if (employee != null)
            {
                userView.employeeID = employee.Id.ToString();
                userView.UserName = employee.UserName;
                userView.Password = employee.Password;
                userView.structureName = employee.OrganizationalStructure.StructureName;
                userView.fullName = employee.Title + employee.FullName;
                userView.imagePath = employee.Photo;
                userView.userRole = employee.Position.ToString();

            }



            return userView;
        }





        [HttpPost]
        [Route("get-affairs")]
        public List<ActiveAffairsViewModel> GetAffair([FromBody] Tbluser emp)
        {



            Guid empId = Guid.Parse(emp.employeeID);

            Employee user = _db.Employees.Include(x => x.OrganizationalStructure).Where(x => x.Id == empId).FirstOrDefault();


            if (user.Position == Position.Secertary)
            {
                var HeadEmployees =
                    _db.Employees.Include(x => x.OrganizationalStructure).Where(
                        x =>
                            x.OrganizationalStructureId == user.OrganizationalStructureId &&
                            x.Position == Position.Director).ToList();
                var allAffairHistory = _db.CaseHistories
                    .Include(x => x.Case)
                    .Include(x => x.FromEmployee)
                    .Include(x => x.FromStructure)
                    .OrderByDescending(x => x.CreatedAt)
                    .Where(x => ((x.ToEmployee.OrganizationalStructureId == user.OrganizationalStructureId &&
                                x.ToEmployee.Position == Position.Director && !x.IsConfirmedBySeretery)
                                || (x.FromEmployee.OrganizationalStructureId == user.OrganizationalStructureId &&
                                x.FromEmployee.Position == Position.Director &&
                                !x.IsForwardedBySeretery &&
                                !x.IsConfirmedBySeretery && x.SecreateryNeeded)
                                ) && x.AffairHistoryStatus != AffairHistoryStatus.Seen && x.AffairHistoryStatus != AffairHistoryStatus.Waiting).Select(x => new ActiveAffairsViewModel
                                {



                                    affairId = x.CaseId,
                                    affairHistoryStatus = x.AffairHistoryStatus.ToString(),
                                    reciverType = x.ReciverType.ToString(),
                                    affairType = x.Case.CaseType.CaseTypeTitle,
                                    remark = x.Remark,
                                    fromStructure = x.FromStructure.StructureName,
                                    affairNumber = x.Case.CaseNumber,
                                    fromEmplyee = x.FromEmployee.FullName,
                                    subject = x.Case.LetterSubject,
                                    applicant = x.Case.Applicant != null ? (x.Case.Applicant.ApplicantName + " / " + x.Case.Applicant.PhoneNumber) : x.Case.Employee.FullName + " / " + x.Case.Employee.PhoneNumber,
                                    createdAt = x.CreatedAt.ToShortDateString(),
                                    historyId = x.Id,
                                    document = _db.CaseAttachments.Where(y => y.CaseId == x.CaseId).Select(d =>
                                        d.FilePath.ToString().Replace('\\', '/')
                                    ).ToList(),
                                    confirmedSecratary = !x.SecreateryNeeded ? (x.IsConfirmedBySeretery ? "Confirmed by " + x.ToEmployee.FullName : "Not Confirmed ") :
                                 x.IsConfirmedBySeretery ? "Confirmed by secretery" : "Not Confirmed"


                                }).ToList();
                ;



                return allAffairHistory;
            }
            else
            {
                var allAffairHistory = _db.CaseHistories
                .Include(x => x.Case)
                .Include(x => x.FromEmployee)
                .Include(x => x.FromStructure)
                .OrderByDescending(x => x.CreatedAt)
                .Where(x => x.AffairHistoryStatus != AffairHistoryStatus.Completed
                            //&& x.AffairHistoryStatus != AffairHistoryStatus.Waiting
                            && x.AffairHistoryStatus != AffairHistoryStatus.Transfered
                            && x.AffairHistoryStatus != AffairHistoryStatus.Revert
                            && x.AffairHistoryStatus != AffairHistoryStatus.Waiting
                            && x.ToEmployeeId == empId).Select(x => new ActiveAffairsViewModel
                            {

                                affairId = x.CaseId,
                                affairHistoryStatus = x.AffairHistoryStatus.ToString(),
                                reciverType = x.ReciverType.ToString(),
                                affairType = x.Case.CaseType.CaseTypeTitle,
                                remark = x.Remark,
                                fromStructure = x.FromStructure.StructureName,
                                affairNumber = x.Case.CaseNumber,
                                fromEmplyee = x.FromEmployee.FullName,
                                subject = x.Case.LetterSubject,
                                applicant = x.Case.Applicant != null ? (x.Case.Applicant.ApplicantName + " / " + x.Case.Applicant.PhoneNumber) : x.Case.Employee.FullName + " / " + x.Case.Employee.PhoneNumber,
                                createdAt = x.CreatedAt.ToShortDateString(),
                                historyId = x.Id,
                                document = _db.CaseAttachments.Where(y => y.CaseId == x.CaseId).Select(d =>
                                    d.FilePath.ToString().Replace('\\', '/')
                                    ).ToList(),
                                confirmedSecratary = !x.SecreateryNeeded ? (x.IsConfirmedBySeretery ? "Confirmed by " + x.ToEmployee.FullName : "Not Confirmed ") :
                                 x.IsConfirmedBySeretery ? "Confirmed by secretery" : "Not Confirmed"

                            }).ToList();

                return allAffairHistory;
            }




        }


        [HttpPost]
        [Route("get-appointments")]
        public List<appointment> getAppointmenr([FromBody] Tbluser emp)
        {
            Guid empId = Guid.Parse(emp.employeeID);
            var appointements = _db.AppointementWithCalender.Where(x => x.EmployeeId == empId).Include(a => a.Case).OrderByDescending(x => x.AppointementDate).ToList();

            var Events = new List<appointment>();
            appointements.ForEach(a =>
            {
                appointment ev = new appointment();


                ev.description = "Appointment with " + a?.Case?.Applicant?.ApplicantName + " at " + a.Time + " Affair Number " + a.Case.CaseNumber;


                ev.appointmentDate = a.AppointementDate;
                ev.name = string.IsNullOrEmpty(a.Remark) ? "Appointment " : a.Remark;


                Events.Add(ev);
            });

            return Events;

        }


        [HttpPost]
        [Route("get-affairHis")]
        public List<CaseHistories> getAffairHis([FromBody] CaseHistories affairHis)
        {

            Guid affairId = Guid.Parse(affairHis.affairId);
            var af = _db.Cases.Find(affairId);

            var affairHistory = _db.CaseHistories
                .Include(a => a.Case)
                .Include(a => a.FromStructure)
                .Include(a => a.FromEmployee)
                .Include(a => a.ToStructure)
                .Include(a => a.ToEmployee)
                .Include(a => a.CaseType)
                .Where(x => x.CaseId == affairId)
                .OrderByDescending(x => x.CreatedAt)
                .ThenBy(x => x.ReciverType).ToList();

            List<CaseHistories> Histories = new List<CaseHistories>();


            foreach (var his in affairHistory)
            {

                CaseHistories history = new CaseHistories();
                history.affairId = his.CaseId.ToString();


                if (his.AffairHistoryStatus == AffairHistoryStatus.Completed)
                {
                    history.status = "Completed";
                }
                else if (his.AffairHistoryStatus == AffairHistoryStatus.Pend && his.ReciverType != ReciverType.Cc)
                {
                    history.status = "Not Seen";
                }
                else if (his.AffairHistoryStatus == AffairHistoryStatus.Seen && his.ReciverType != ReciverType.Cc)
                {
                    history.status = "Seen";
                }
                else if (his.AffairHistoryStatus == AffairHistoryStatus.Transfered)
                {
                    history.status = "Transfered";
                }
                else if (his.AffairHistoryStatus == AffairHistoryStatus.Revert)
                {
                    history.status = "Reverted";
                }
                history.affairHisId = his.Id.ToString();
                history.affairType = his.ReciverType.ToString();
                history.employeeId = affairHis.employeeId;
                history.fromStructure = his.FromStructure.StructureName;
                history.fromEmployee = his.FromEmployee.FullName;
                history.toStructure = his.ToStructure.StructureName;
                history.toEmployeeId = his.ToEmployeeId.ToString();
                history.toEmployee = his.ToEmployee.FullName;
                history.messageStatus = (his.IsSmsSent != true && his.ReciverType == ReciverType.Orginal) ? "Not Sent" : "Sent";

                string givenDate = his.CreatedAt.ToString("dd/MM/yyyy");
                string[] givenDateToArray = givenDate.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                var CreatedDateTime = EthiopicDateTime.GetEthiopicDate(Int32.Parse(givenDateToArray[0]), Int32.Parse(givenDateToArray[1]), Int32.Parse(givenDateToArray[2]));

                history.datetime = CreatedDateTime + " " + his.CreatedAt.ToString("h:mm");


                var affairTypes = _db.CaseTypes.Where(x => x.ParentCaseTypeId == af.CaseTypeId).ToList();
                foreach (var childaffair in affairTypes)
                {
                    int childcount = his.childOrder;

                    if (childaffair.OrderNumber == childcount)
                    {
                        history.title = childaffair.CaseTypeTitle;
                    }


                }
                history.title += " " + his.ReciverType;


                Histories.Add(history);
            }




            return Histories;

        }

        [HttpPost]
        [Route("get-affairWaiting")]
        public List<ActiveAffairsViewModel> getAffairList([FromBody] Tbluser affairWait)
        {
            var currentUser = Guid.Parse(affairWait.employeeID);

            var allAffairHistory = _db.CaseHistories
            .Include(x => x.Case)
            .Include(x => x.FromEmployee)
            .Include(x => x.FromStructure)
            .OrderByDescending(x => x.CreatedAt)
            .Where(x => x.AffairHistoryStatus == AffairHistoryStatus.Waiting
                        && x.ToEmployeeId == currentUser).Select(x => new ActiveAffairsViewModel
                        {


                            affairId = x.CaseId,
                            affairHistoryStatus = x.AffairHistoryStatus.ToString(),
                            reciverType = x.ReciverType.ToString(),
                            affairType = x.Case.CaseType.CaseTypeTitle,
                            remark = x.Remark,
                            fromStructure = x.FromStructure.StructureName,
                            affairNumber = x.Case.CaseNumber,
                            fromEmplyee = x.FromEmployee.FullName,
                            subject = x.Case.LetterSubject,
                            applicant = x.Case.Applicant != null ? x.Case.Applicant.ApplicantName : x.Case.Employee.FullName,
                            createdAt = x.CreatedAt.ToShortDateString(),
                            historyId = x.Id,
                            document = _db.CaseAttachments.Where(y => y.CaseId == x.CaseId).Select(d =>
                                d.FilePath.ToString()
                                    ).ToList(),
                            confirmedSecratary = !x.SecreateryNeeded ? (x.IsConfirmedBySeretery ? "Confirmed by " + x.ToEmployee.FullName : "Not Confirmed ") :
                                 x.IsConfirmedBySeretery ? "Confirmed by secretery" : "Not Confirmed"

                        }).ToList();

            return allAffairHistory;
        }


        [HttpPost]
        [Route("Add-to-WaitingList")]
        public string addToWaitingList([FromBody] CaseHistories affirHistory)
        {

            try
            {

                var currentUser = Guid.Parse(affirHistory.employeeId);
                var affairId = Guid.Parse(affirHistory.affairId);
                var affairHisId = Guid.Parse(affirHistory.affairHisId);
                var history = _db.CaseHistories.Find(affairHisId);
                history.AffairHistoryStatus = AffairHistoryStatus.Waiting;
                history.SeenDateTime = null;
                _db.CaseHistories.Attach(history);
                _db.Entry(history).Property(c => c.AffairHistoryStatus).IsModified = true;
                _db.Entry(history).Property(c => c.SeenDateTime).IsModified = true;
                _db.SaveChanges();

                return "Successfully added to Waiting List";

            }
            catch (Exception e)
            {


                return "Something went Wrong";
            }




        }

        [HttpPost]
        [Route("make-appointment")]
        public async Task<string> makeappointments([FromBody] makeappointment appointment)
        {




            try
            {
                var currentUser = Guid.Parse(appointment.employeeId);
                var empId = _userManager.Users.Where(x => x.EmployeesId == currentUser).FirstOrDefault().Id;

                var gerdate = appointment.executionDate.Split('/');
                var ethipianDate = EthiopicDateTime.GetEthiopicDate(int.Parse(gerdate[0]), int.Parse(gerdate[1]), int.Parse(gerdate[2]));
                var splittedDate = ethipianDate.Split('/');

                ethipianDate = splittedDate[1] + "/" + splittedDate[0] + "/" + splittedDate[2];


                AppointementWithCalender appointmen = new()
                {
                    Id = Guid.NewGuid(),
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = Guid.Parse(empId),
                    AppointementDate = ethipianDate,
                    CaseId = Guid.Parse(appointment.affairId),
                    Time = appointment.executionTime,
                    EmployeeId = Guid.Parse(appointment.employeeId),
                };

                PM_Case_Managemnt_Infrustructure.Models.CaseModel.Case cases = _db.Cases.Include(x => x.Applicant).Where(x => x.Id == appointmen.CaseId).FirstOrDefault();

                string message = cases.Applicant.ApplicantName + " ለጉዳይ ቁጥር፡ " + cases.CaseNumber + "\n በ " + ethipianDate + " ቀን በ " + appointmen.Time +
                 " ሰዐት በቢሮ ቁጥር፡ - ይገኙ";


                bool isSmssent = await _smshelper.UnlimittedMessageSender(cases.Applicant.PhoneNumber, message, empId.ToString());

                if (!isSmssent)
                    _smshelper.UnlimittedMessageSender(cases.PhoneNumber2, message, empId.ToString());

                _db.AppointementWithCalender.Add(appointmen);
                _db.SaveChanges();



                return "Successfully appointed ";
            }
            catch (Exception e)
            {


                return "Something went wrong";

            }


        }


        [HttpPost]
        [Route("complete-affair")]
        public async Task<string> completeAffairs([FromBody] completeAffair completeAffair)
        {

            try

            {

                Guid affairHIsId = Guid.Parse(completeAffair.affairHisId);
                var currentUser = Guid.Parse(completeAffair.employeeId);
                var empId = _userManager.Users.Where(x => x.EmployeesId == currentUser).FirstOrDefault().Id;

                CaseCompleteDto caseCompleteDto = new CaseCompleteDto
                {
                    CaseHistoryId = Guid.Parse(completeAffair.affairHisId),
                    Remark = completeAffair.Remark,
                    EmployeeId = Guid.Parse(completeAffair.employeeId)

                };









                CaseHistory selectedHistory = _db.CaseHistories.Find(caseCompleteDto.CaseHistoryId);
                Guid UserId = Guid.Parse((await _userManager.Users.Where(appUsr => appUsr.EmployeesId.Equals(selectedHistory.ToEmployeeId)).FirstAsync()).Id);

                if (selectedHistory.ToEmployeeId != caseCompleteDto.EmployeeId)
                    throw new Exception("You are unauthorized to complete this case.");


                selectedHistory.AffairHistoryStatus = AffairHistoryStatus.Completed;
                selectedHistory.CompletedDateTime = DateTime.Now;
                selectedHistory.Remark = caseCompleteDto.Remark;


                PM_Case_Managemnt_Infrustructure.Models.CaseModel.Case currentCase = await _db.Cases.Include(x => x.Applicant).Include(x => x.Employee).FirstOrDefaultAsync(x => x.Id == selectedHistory.CaseId);
                CaseHistory currentHist = await _db.CaseHistories.Include(x => x.Case).Include(x => x.ToStructure).FirstOrDefaultAsync(x => x.Id == selectedHistory.Id);

                _db.CaseHistories.Attach(selectedHistory);
                _db.Entry(selectedHistory).Property(x => x.AffairHistoryStatus).IsModified = true;
                _db.Entry(selectedHistory).Property(x => x.CompletedDateTime).IsModified = true;
                _db.Entry(selectedHistory).Property(x => x.Remark).IsModified = true;
                //_db.Entry(selectedHistory).Property(x => x.IsSmsSent).IsModified = true;
                //_db.SaveChanges();

                var selectedCase = _db.Cases.Find(selectedHistory.CaseId);
                selectedCase.CompletedAt = DateTime.Now;
                selectedCase.AffairStatus = AffairStatus.Completed;

                _db.Cases.Attach(selectedCase);
                _db.Entry(selectedCase).Property(x => x.CompletedAt).IsModified = true;
                _db.Entry(selectedCase).Property(x => x.AffairStatus).IsModified = true;

                await _db.SaveChangesAsync();

                string name = currentCase.Applicant != null ? currentCase.Applicant.ApplicantName : currentCase.Employee.FullName;
                string message = name + "\nበጉዳይ ቁጥር፡" + currentCase.CaseNumber + "\nየተመዘገበ ጉዳዮ በ፡" + currentHist.ToStructure.StructureName + " ተጠናቋል\nየቢሮ ቁጥር: - ";

                await _smshelper.SendSmsForCase(message, currentHist.CaseId, currentHist.Id, UserId.ToString(), MessageFrom.Complete);




                return "Successfully complete Case";
            }
            catch (Exception e)
            {
                return "Something Went Wrong";
            }
        }

        [HttpPost]
        [Route("Revert-affair")]

        public async Task<string> RevertAffair([FromBody] completeAffair completeAffair)
        {
            try
            {

                CaseRevertDto revertCase = new CaseRevertDto
                {
                    EmployeeId = Guid.Parse(completeAffair.employeeId),
                    CaseHistoryId = Guid.Parse(completeAffair.affairHisId),
                    Remark = completeAffair.Remark

                };



                Employee currEmp = await _db.Employees.Include(x => x.OrganizationalStructure).Where(x => x.Id == (revertCase.EmployeeId)).FirstOrDefaultAsync();
                CaseHistory selectedHistory = _db.CaseHistories.Find(revertCase.CaseHistoryId);
                Guid UserId = Guid.Parse((await _userManager.Users.Where(appUsr => appUsr.EmployeesId.Equals(selectedHistory.ToEmployeeId)).FirstAsync()).Id);

                selectedHistory.AffairHistoryStatus = AffairHistoryStatus.Revert;
                selectedHistory.RevertedAt = DateTime.Now;
                selectedHistory.Remark = revertCase.Remark;

                _db.CaseHistories.Attach(selectedHistory);
                _db.Entry(selectedHistory).Property(x => x.AffairHistoryStatus).IsModified = true;
                _db.Entry(selectedHistory).Property(x => x.RevertedAt).IsModified = true;
                _db.Entry(selectedHistory).Property(x => x.Remark).IsModified = true;
                CaseHistory newHistory = new()
                {
                    Id = Guid.NewGuid(),
                    CreatedAt = DateTime.Now,
                    CreatedBy = Guid.Parse(_userManager.Users.Where(appUsr => appUsr.EmployeesId.Equals(revertCase.EmployeeId)).First().Id),
                    RowStatus = RowStatus.Active,
                    FromEmployeeId = revertCase.EmployeeId,
                    FromStructureId = currEmp.OrganizationalStructureId,
                    ToEmployeeId = selectedHistory.FromEmployeeId,
                    ToStructureId = selectedHistory.FromStructureId,
                    Remark = "",
                    CaseId = selectedHistory.CaseId,
                    ReciverType = ReciverType.Orginal,
                    childOrder = selectedHistory.childOrder += 1,
                };


                await _db.CaseHistories.AddAsync(newHistory);
                await _db.SaveChangesAsync();

                PM_Case_Managemnt_Infrustructure.Models.CaseModel.Case currentCase = await _db.Cases.Include(x => x.Applicant).Include(x => x.Employee).FirstOrDefaultAsync(x => x.Id == selectedHistory.CaseId);
                string name = currentCase.Applicant != null ? currentCase.Applicant.ApplicantName : currentCase.Employee.FullName;
                var message = name + "\nበጉዳይ ቁጥር፡" + currentCase.CaseNumber + "\nየተመዘገበ ጉዳዮ በ፡" + selectedHistory.ToStructure.StructureName + " ወደኋላ ተመልሷል  \nየቢሮ ቁጥር: -";

                // await _smshelper.SendSmsForCase(message, newHistory.CaseId, newHistory.Id, UserId.ToString(), MessageFrom.Revert);


                return "Successfully reverted ";

            }

            catch
            {

                return "Something went wrong";
            }
        }

        [HttpPost]
        [Route("get-results")]
        public results getResults([FromBody] results affairhis)
        {


            var historyDetailId = Guid.Parse(affairhis.historyDetailId);
            var Results = new results();
            Results.historyDetailId = affairhis.historyDetailId;

            var affairHistory = _db.CaseHistories.Include(x => x.Case).Where(x => x.Id == historyDetailId).FirstOrDefault();
            var affairTypeId = affairHistory.Case.CaseTypeId;


            Results.affairType = _db.CaseTypes.Find(affairTypeId).CaseTypeTitle;
            var affairtypes = _db.CaseTypes.Where(x => x.ParentCaseTypeId == affairTypeId).ToList();

            var fileSetting = _db.FileSettings.Include(x => x.CaseType.ParentCaseType).Where(x => x.CaseTypeId == affairTypeId).ToList();

            foreach (var affairtype in affairtypes)
            {
                fileSetting.AddRange(_db.FileSettings.Include(x => x.CaseType.ParentCaseType).Where(x => x.CaseTypeId == affairtype.Id).ToList());
            }

            if (fileSetting.Any() && fileSetting.FirstOrDefault().CaseTypeId != null)
            {

                Results.affairType = fileSetting.FirstOrDefault().CaseType.CaseTypeTitle;
            }
            if (fileSetting.Any() && fileSetting.FirstOrDefault().CaseType.ParentCaseTypeId != null)
            {

                Results.affairType = fileSetting.FirstOrDefault().CaseType.ParentCaseType.CaseTypeTitle;
            }

            foreach (var childaffair in affairtypes)
            {
                int childcount = affairHistory.childOrder + 1;

                if (childaffair.OrderNumber == childcount)
                {

                    Results.currentState = childaffair.CaseTypeTitle;


                    foreach (var file in fileSetting)
                    {

                        if (childaffair.Id == file.CaseType.Id)
                        {
                            Results.neededDocuments += file.FileName;
                        }
                    }
                }
                if (childaffair.OrderNumber == childcount + 1)
                {

                    Results.nextState = childaffair.CaseTypeTitle;



                }

            }

            // Results.branchs = new List<StructureViewModel>();

            Results.employees = new List<EmployeeViewModel>();
            Results.structures = new List<StructureViewModel>();


            var employees = _db.Employees.Include(x => x.OrganizationalStructure).Select(emp => new EmployeeViewModel
            {
                empId = emp.Id.ToString(),
                empName = emp.FullName + " (" + emp.OrganizationalStructure.StructureName + ")"
            }).ToList(); ;
            var structures = _db.OrganizationalStructures
                .Select(y => new StructureViewModel
                {
                    structureName = y.StructureName,
                    strucutreId = y.Id.ToString()
                }).ToList();
            ;
            var branch = _db.OrganizationalStructures.Where(u => u.IsBranch).Select(y => new StructureViewModel
            {
                structureName = y.StructureName,
                strucutreId = y.Id.ToString()
            }).ToList();

            //  Results.branchs = branch;
            Results.employees = employees;
            Results.structures = structures;





            return Results;

        }


        [HttpPost]
        [Route("Transfer-affair")]

        public async Task<string> TransferAffair([FromForm] TransferAffairRequest request)
        {
            try
            {



                Employee currEmp = await _db.Employees.Where(el => el.Id == request.empIdd).FirstOrDefaultAsync();
                CaseHistory currentLastHistory = await _db.CaseHistories.Where(el => el.Id == request.affairHisIdd).OrderByDescending(x => x.CreatedAt).FirstOrDefaultAsync();

                Guid UserId = Guid.Parse((await _userManager.Users.Where(appUsr => appUsr.EmployeesId == request.empIdd).FirstOrDefaultAsync()).Id);

                if (request.empIdd != currentLastHistory.ToEmployeeId)
                    throw new Exception("You are not authorized to transfer this case.");

                currentLastHistory.AffairHistoryStatus = AffairHistoryStatus.Transfered;
                currentLastHistory.TransferedDateTime = DateTime.Now;

                _db.CaseHistories.Attach(currentLastHistory);
                _db.Entry(currentLastHistory).Property(c => c.AffairHistoryStatus).IsModified = true;
                _db.Entry(currentLastHistory).Property(c => c.TransferedDateTime).IsModified = true;

                var toEmployee = request.toEmployeeId == Guid.Empty || request.toEmployeeId == null ?
         _db.Employees.FirstOrDefault(
           e =>
               e.OrganizationalStructureId == request.toStructureId &&
               e.Position == Position.Director).Id : request.toEmployeeId;


                var toStructure = request.toStructureId == Guid.Empty || request.toStructureId == null ? _db.Employees.Find(toEmployee).OrganizationalStructureId : request.toStructureId;


                var newHistory = new CaseHistory
                {
                    Id = Guid.NewGuid(),
                    CreatedAt = DateTime.Now,
                    CreatedBy = UserId,

                    RowStatus = RowStatus.Active,
                    FromEmployeeId = request.empIdd,
                    FromStructureId = currEmp.OrganizationalStructureId,
                    ToEmployeeId = toEmployee,
                    ToStructureId = toStructure,
                    Remark = request.remark,
                    CaseId = currentLastHistory.CaseId,
                    ReciverType = ReciverType.Orginal,
                    CaseTypeId = currentLastHistory.CaseTypeId,
                    childOrder = currentLastHistory.childOrder + 1
                    //must be change
                };


                await _db.CaseHistories.AddAsync(newHistory);
                await _db.SaveChangesAsync();


                if (request.photo != null)
                {
                    string folderName = Path.Combine("Assets", "CaseAttachments");
                    string pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);

                    //Create directory if not exists
                    if (!Directory.Exists(pathToSave))
                        Directory.CreateDirectory(pathToSave);

                    if (request.photo.Length > 0)
                    {
                        string fileName = ContentDispositionHeaderValue.Parse(request.photo.ContentDisposition).FileName.Trim('"');
                        string fullPath = Path.Combine(pathToSave, fileName);
                        string dbPath = Path.Combine(folderName, fileName);

                        using (var stream = new FileStream(fullPath, FileMode.Create))
                        {
                            request.photo.CopyTo(stream);
                        }
                        CaseAttachment attachment = new()
                        {
                            Id = Guid.NewGuid(),
                            CreatedAt = DateTime.Now,
                            CreatedBy = UserId,
                            RowStatus = RowStatus.Active,
                            CaseId = currentLastHistory.CaseId,
                            FilePath = dbPath
                        };
                        await _db.CaseAttachments.AddAsync(attachment);
                        await _db.SaveChangesAsync();

                    }
                }

                /// Sending SMS
                PM_Case_Managemnt_Infrustructure.Models.CaseModel.Case currentCase = await _db.Cases.Include(x => x.Applicant).Include(x => x.Employee).FirstOrDefaultAsync(x => x.Id == newHistory.CaseId);
                string name = currentCase.Applicant != null ? currentCase.Applicant.ApplicantName : currentCase.Employee.FullName;
                string toStructureName = _db.OrganizationalStructures.Find(newHistory.ToStructureId).StructureName;

                string message = name + "\nበጉዳይ ቁጥር፡" + currentCase.CaseNumber + "\nየተመዘገበ ጉዳዮ ለ " + toStructureName + " ተላልፏል\nየቢሮ ቁጥር:";
                await _smshelper.SendSmsForCase(message, newHistory.CaseId, newHistory.Id, UserId.ToString(), MessageFrom.Transfer);

                return "Successfully Transfred";

            }
            catch (Exception e)
            {
                return e.Message;
            }



        }

        [HttpPost]
        [Route("send-message")]
        public async Task<string> SendSMS([FromBody] messageViewModel message)
        {


            try
            {

                var currentUser = Guid.Parse(message.employeeId);
                var empId = _userManager.Users.Where(x => x.EmployeesId == currentUser).FirstOrDefault().Id;
                Guid affairId = Guid.Parse(message.affairId);

                var affair = _db.Cases.Find(affairId);
                var currcaseHist = _db.CaseHistories.Where(x => x.CaseId == affairId).OrderByDescending(x => x.childOrder).FirstOrDefault();
                if (affair != null)
                {

                    var applicant = _db.Applicants.Find(affair.ApplicantId);
                    if (applicant != null)
                    {




                        await _smshelper.SendSmsForCase(message.message, affairId, currcaseHist.Id, empId, MessageFrom.Custom_text);


                    }



                }

                return "successfully Sent ";
            }
            catch (Exception e)
            {
                string sdf = e.Message;
                return "Sending message was not successfull ";

            }







        }


        [HttpPost]
        [Route("get-notification")]
        public NotificationCount getNotfi([FromBody] Tbluser NotCount)
        {


            var notficationCout = new NotificationCount();

            notficationCout.affairCount = GetAffair(NotCount).Count(x => x.affairHistoryStatus == "Seen");
            notficationCout.waitingListCount = getAffairList(NotCount).Count();
            notficationCout.appointmentCount = getAppointmenr(NotCount).Count(x => x.appointmentDate.Split('T')[0] == DateTime.Now.ToString("yyyy-MM-dd"));




            return notficationCout;


        }


        public class TransferAffairRequest
        {
            public IFormFile? photo { get; set; }
            public Guid? empIdd { get; set; }
            public Guid? affairHisIdd { get; set; }
            public Guid? toEmployeeId { get; set; }
            public Guid? toStructureId { get; set; }
            public string? remark { get; set; }
            public string? caseTypeId { get; set; }
        }
        public class NotificationCount
        {
            public int affairCount { get; set; }
            public int appointmentCount { get; set; }
            public int waitingListCount { get; set; }
            public string employeeId { get; set; }

        }
        public class Tbluser
        {
            public string? employeeID { get; set; }
            public string? UserName { get; set; }
            public string? Password { get; set; }
            public string? structureName { get; set; }
            public string? fullName { get; set; }
            public string? imagePath { get; set; }
            public string? userRole { get; set; }


        }
        public class results
        {
            public string historyDetailId { get; set; }
            public string? affairType { get; set; }
            public string? nextState { get; set; }
            public string? currentState { get; set; }
            public string? neededDocuments { get; set; }
            public List<EmployeeViewModel>? employees { get; set; }
            public List<StructureViewModel>? structures { get; set; }

            public List<StructureViewModel>? branchs { get; set; }
        }
        public class transferAffair
        {

            public string empIdd { get; set; }
            public string affairHisIdd { get; set; }
            public string toEmployeeId { get; set; }
            public string toStructureId { get; set; }
            public string remark { get; set; }
            public string caseTypeId { get; set; }


        }


        public class StructureViewModel
        {
            public string structureName { get; set; }
            public string strucutreId { get; set; }
        }
        public class EmployeeViewModel
        {
            public string empName { get; set; }
            public string empId { get; set; }
        }

        public class completeAffair
        {
            public string employeeId { get; set; }
            public string affairHisId { get; set; }
            public string Remark { get; set; }
        }
        public class makeappointment
        {
            public string employeeId { get; set; }
            public string executionDate { get; set; }
            public string executionTime { get; set; }
            public string affairId { get; set; }


        }

        public class CaseHistories
        {
            public string? affairHisId { get; set; }
            public string? employeeId { get; set; }
            public string? toEmployeeId { get; set; }
            public string? affairType { get; set; }
            public string affairId { get; set; }
            public string? datetime { get; set; }
            public string? title { get; set; }
            public string? fromStructure { get; set; }
            public string? toStructure { get; set; }
            public string? fromEmployee { get; set; }
            public string? toEmployee { get; set; }
            public string? status { get; set; }
            public string? messageStatus { get; set; }
        }

        public class appointment
        {

            public string description { get; set; }
            public string appointmentDate { get; set; }
            public string name { get; set; }
        }


        public class messageViewModel
        {

            public string message { get; set; }
            public string affairId { get; set; }
            public string employeeId { get; set; }

        }
    }
}

