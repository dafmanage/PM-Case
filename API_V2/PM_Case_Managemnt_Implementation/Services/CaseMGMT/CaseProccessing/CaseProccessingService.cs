using System.Net;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using PM_Case_Managemnt_Implementation.DTOS.CaseDto;
using PM_Case_Managemnt_Implementation.DTOS.Common;
using PM_Case_Managemnt_Implementation.Helpers;
using PM_Case_Managemnt_Implementation.Helpers.Logger;
using PM_Case_Managemnt_Implementation.Helpers.Response;
using PM_Case_Managemnt_Implementation.Hubs.EncoderHub;
using PM_Case_Managemnt_Implementation.Services.CaseService.Encode;
using PM_Case_Managemnt_Infrustructure.Data;
using PM_Case_Managemnt_Infrustructure.Models.Auth;
using PM_Case_Managemnt_Infrustructure.Models.CaseModel;
using PM_Case_Managemnt_Infrustructure.Models.Common;
using PM_Case_Managemnt_Infrustructure.Models.PM;

namespace PM_Case_Managemnt_Implementation.Services.CaseMGMT
{
    public class CaseProccessingService : ICaseProccessingService
    {

        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ISMSHelper _smshelper;
        private readonly ICaseEncodeService _caseEncodeService;
        private IHubContext<EncoderHub, IEncoderHubInterface> _encoderHub;
        private readonly ILoggerManagerService _logger;

        public CaseProccessingService(
            ApplicationDbContext dbContext,
            UserManager<ApplicationUser> userManager,
            ISMSHelper smshelper,
            ICaseEncodeService caseEncodeService,
            IHubContext<EncoderHub, IEncoderHubInterface> encoderHub,
            ILoggerManagerService logger)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _smshelper = smshelper;
            _caseEncodeService = caseEncodeService;
            _encoderHub = encoderHub;
            _logger = logger;

        }

        public async Task<ResponseMessage<int>> ConfirmTranasaction(ConfirmTranscationDto confirmTranscationDto)
        {
            var response = new ResponseMessage<int>();

            try
            {

                var history = _dbContext.CaseHistories.Find(confirmTranscationDto.CaseHistoryId);
                
                if (history == null){
                    
                    response.Message = "Couldnt find history";
                    response.Data = 0;
                    response.ErrorCode = HttpStatusCode.NotFound.ToString();
                    response.Success = false;
                    
                    return response;
                }

                history.IsConfirmedBySeretery = true;
                history.SecreteryConfirmationDateTime = DateTime.Now;
                history.SecreteryId = confirmTranscationDto.EmployeeId;

                _dbContext.CaseHistories.Attach(history);
                _dbContext.Entry(history).Property(c => c.IsConfirmedBySeretery).IsModified = true;
                _dbContext.Entry(history).Property(c => c.SecreteryConfirmationDateTime).IsModified = true;
                _dbContext.Entry(history).Property(c => c.SecreteryId).IsModified = true;
                _dbContext.SaveChanges();

                response.Success = true;
                response.Data = 1;
                response.Message = "Successfull";
                _logger.LogCreate("Case Processing Service", confirmTranscationDto.EmployeeId.ToString(), "Transaction confirmed Successfully");
                return response;
            }
            catch (Exception ex)
            {
                response.Message = $"{ex.Message}";
                response.Data = 0;
                response.ErrorCode = HttpStatusCode.InternalServerError.ToString();
                response.Success = false;

                return response;
            }
        }

        public async Task<ResponseMessage<int>> AssignTask(CaseAssignDto caseAssignDto)
        {
            var response = new ResponseMessage<int>();
            try
            {
                var user = _userManager.Users.Where(x => x.EmployeesId == caseAssignDto.AssignedByEmployeeId).FirstOrDefault();
                if (user == null){

                    response.Message = "Couldnt find user.";
                    response.Data = 0;
                    response.ErrorCode = HttpStatusCode.NotFound.ToString();
                    response.Success = false;

                    return response;
                }

                string userId = user.Id;
                Case? caseToAssign = await _dbContext.Cases.SingleOrDefaultAsync(el => el.Id.Equals(caseAssignDto.CaseId));
                // CaseHistory caseHistory = await _dbContext.CaseHistories.SingleOrDefaultAsync(el => el.CaseId.Equals(caseAssignDto.CaseId));
                
                if (caseToAssign == null){

                    response.Message = "Couldnt find case to assign";
                    response.Data = 0;
                    response.ErrorCode = HttpStatusCode.NotFound.ToString();
                    response.Success = false;

                    return response;
                }
                
                var toEmployeeId = caseAssignDto.AssignedToEmployeeId == Guid.Empty || caseAssignDto.AssignedToEmployeeId == null ?
             _dbContext.Employees.FirstOrDefault(
                 e =>
                     e.OrganizationalStructureId == caseAssignDto.AssignedToStructureId &&
                     e.Position == Position.Director).Id : caseAssignDto.AssignedToEmployeeId;

                var fromEmployeestructure = _dbContext.Employees.Include(x => x.OrganizationalStructure).Where(x => x.Id == caseAssignDto.AssignedByEmployeeId).First().OrganizationalStructure.Id;

                Case? currCase = await _dbContext.Cases.SingleOrDefaultAsync(el => el.Id.Equals(caseAssignDto.CaseId));
                if (currCase == null){

                    response.Message = "Couldnt find current case.";
                    response.Data = 0;
                    response.ErrorCode = HttpStatusCode.NotFound.ToString();
                    response.Success = false;

                    return response;
                }
                currCase.AffairStatus = AffairStatus.Assigned;

                _dbContext.Entry(currCase).Property(curr => curr.AffairStatus).IsModified = true;
                //await _dbContext.SaveChangesAsync();
                if (caseAssignDto.ForwardedToStructureId != null)
                {
                    var startupHistory = new CaseHistory
                    {
                        Id = Guid.NewGuid(),
                        CreatedAt = DateTime.Now,
                        CreatedBy = Guid.Parse(userId),
                        RowStatus = RowStatus.Active,
                        CaseId = caseAssignDto.CaseId,
                        CaseTypeId = _dbContext.Cases.Find(caseAssignDto.CaseId).CaseTypeId,
                        AffairHistoryStatus = AffairHistoryStatus.Pend,
                        FromEmployeeId = caseAssignDto.AssignedByEmployeeId,
                        FromStructureId = fromEmployeestructure,
                        ReciverType = ReciverType.Orginal,
                        ToStructureId = caseAssignDto.AssignedToStructureId,
                        ToEmployeeId = toEmployeeId,

                    };
                    startupHistory.SecreateryNeeded = (caseAssignDto.AssignedToEmployeeId == Guid.Empty || caseAssignDto.AssignedToEmployeeId == null) ? true : false;

                    caseToAssign.AffairStatus = AffairStatus.Assigned;
                    _dbContext.Entry(caseToAssign).Property(x => x.AffairStatus).IsModified = true;

                    await _dbContext.CaseHistories.AddAsync(startupHistory);
                    await _dbContext.SaveChangesAsync();

                    foreach (var row in caseAssignDto.ForwardedToStructureId)
                    {

                        var toEmployee = new Guid();
                        if (_dbContext.Employees.FirstOrDefault(
                                 e =>
                                     e.OrganizationalStructureId == row &&
                                     e.Position == Position.Director) == null)
                        {
                            toEmployee = Guid.Empty;
                        }
                        else
                        {
                            toEmployee = _dbContext.Employees.FirstOrDefault(
                                 e =>
                                     e.OrganizationalStructureId == row &&
                                     e.Position == Position.Director).Id;
                        }

                        CaseHistory history = new()
                        {
                            Id = Guid.NewGuid(),
                            CreatedAt = DateTime.Now,
                            CreatedBy = Guid.Parse(userId),
                            CaseId = caseAssignDto.CaseId,
                            AffairHistoryStatus = AffairHistoryStatus.Pend,
                            FromEmployeeId = caseAssignDto.AssignedByEmployeeId,
                            FromStructureId = fromEmployeestructure,
                            ToStructureId = row,
                            ReciverType = ReciverType.Cc,
                            ToEmployeeId = toEmployee,
                            RowStatus = RowStatus.Active,

                        };
                        await _dbContext.CaseHistories.AddAsync(history);
                        await _dbContext.SaveChangesAsync();


                    }
                }
                var assigndCase = await _caseEncodeService.GetAllTransfred(toEmployeeId.Value);


                await _encoderHub.Clients.Group(toEmployeeId.Value.ToString()).getNotification(assigndCase.Data, toEmployeeId.Value.ToString());

                //await _encoderHub.Clients.All.getNotification(assigndCase);
                response.Message = "operation Successful";
                response.Data = 1;
                response.Success = true;
                _logger.LogCreate("CaseProcessingService", caseAssignDto.AssignedByEmployeeId.ToString(), $"case assigned to {caseAssignDto.AssignedToEmployeeId} Successfully");
                return response;

            }
            catch (Exception ex)
            {
                response.Message = $"{ex.Message}";
                response.Data = 0;
                response.ErrorCode = HttpStatusCode.InternalServerError.ToString();
                response.Success = false;

                return response;
            }
        }

        public async Task<ResponseMessage<int>> CompleteTask(CaseCompleteDto caseCompleteDto)
        {

            var response = new ResponseMessage<int>();
            try
            {
                CaseHistory? selectedHistory = _dbContext.CaseHistories.Find(caseCompleteDto.CaseHistoryId);
                if (selectedHistory == null){

                    response.Message = "Couldnt find history";
                    response.Data = 0;
                    response.ErrorCode = HttpStatusCode.NotFound.ToString();
                    response.Success = false;
                    
                    return response;
                }
                Guid UserId = Guid.Parse((await _userManager.Users.Where(appUsr => appUsr.EmployeesId.Equals(selectedHistory.ToEmployeeId)).FirstAsync()).Id);
                
                CaseHistory selectedHistoryCC = _dbContext.CaseHistories.Where(x => x.CaseId == selectedHistory.CaseId && x.ReciverType == ReciverType.Cc).FirstOrDefault();
                if (selectedHistory.ToEmployeeId != caseCompleteDto.EmployeeId){

                    response.Message = "You are unauthorized to complete this case.";
                    response.Data = 0;
                    response.ErrorCode = HttpStatusCode.MethodNotAllowed.ToString();
                    response.Success = false;

                    return response;
                }


                selectedHistory.AffairHistoryStatus = AffairHistoryStatus.Completed;
                selectedHistory.CompletedDateTime = DateTime.Now;
                selectedHistory.Remark = caseCompleteDto.Remark;

                selectedHistoryCC.AffairHistoryStatus = AffairHistoryStatus.Completed;
                selectedHistoryCC.CompletedDateTime = DateTime.Now;
                selectedHistoryCC.Remark = caseCompleteDto.Remark;

                Case? currentCase = await _dbContext.Cases.Include(x => x.Applicant).Include(x => x.Employee).FirstOrDefaultAsync(x => x.Id == selectedHistory.CaseId);
                CaseHistory? currentHist = await _dbContext.CaseHistories.Include(x => x.Case).Include(x => x.ToStructure).FirstOrDefaultAsync(x => x.Id == selectedHistory.Id);
                if (currentCase ==  null || currentHist == null){

                    response.Message = "Empty current Case or Empty current History";
                    response.Data = 0;
                    response.ErrorCode = HttpStatusCode.NotFound.ToString();
                    response.Success = false;

                    return response;
                    
                }
                _dbContext.CaseHistories.Attach(selectedHistory);
                _dbContext.Entry(selectedHistory).Property(x => x.AffairHistoryStatus).IsModified = true;
                _dbContext.Entry(selectedHistory).Property(x => x.CompletedDateTime).IsModified = true;
                _dbContext.Entry(selectedHistory).Property(x => x.Remark).IsModified = true;

                _dbContext.CaseHistories.Attach(selectedHistoryCC);
                _dbContext.Entry(selectedHistoryCC).Property(x => x.AffairHistoryStatus).IsModified = true;
                _dbContext.Entry(selectedHistoryCC).Property(x => x.CompletedDateTime).IsModified = true;
                _dbContext.Entry(selectedHistoryCC).Property(x => x.Remark).IsModified = true;
                //_dbContext.Entry(selectedHistory).Property(x => x.IsSmsSent).IsModified = true;
                //_dbContext.SaveChanges();

                var selectedCase = _dbContext.Cases.Find(selectedHistory.CaseId);
                if (selectedCase == null){

                    response.Message = "Selected Case is Empty";
                    response.Data = 0;
                    response.ErrorCode = HttpStatusCode.NotFound.ToString();
                    response.Success = false;

                    return response;
                }
                selectedCase.CompletedAt = DateTime.Now;
                selectedCase.AffairStatus = AffairStatus.Completed;

                _dbContext.Cases.Attach(selectedCase);
                _dbContext.Entry(selectedCase).Property(x => x.CompletedAt).IsModified = true;
                _dbContext.Entry(selectedCase).Property(x => x.AffairStatus).IsModified = true;

                await _dbContext.CaseAttachments.AddRangeAsync(caseCompleteDto.CaseAttachments);

                await _dbContext.SaveChangesAsync();

                var employees = await _dbContext.Employees.Include(x => x.OrganizationalStructure).Where(x => x.Id == selectedHistory.ToEmployeeId).ToListAsync();
                if (employees.Any())

                {
                    var employee = employees.FirstOrDefault();

                    var activities = await _dbContext.Activities.Where(x => x.CaseTypeId == currentCase.CaseTypeId && x.OrganizationalStructureId == employee.OrganizationalStructure.OrganizationBranchId).ToListAsync();

                    if (activities.Any())
                    {
                        var activity = activities.FirstOrDefault();
                        var actTarget = await _dbContext.ActivityTargetDivisions.Where(x => x.ActivityId == activity.Id).ToListAsync();
                        if (activity != null && actTarget.Any())
                        {

                            var activityProgress2 = new ActivityProgress
                            {
                                Id = Guid.NewGuid(),
                                CreatedAt = DateTime.Now,
                                QuarterId = actTarget.FirstOrDefault().Id,
                                ActualBudget = 0,
                                ActualWorked = 1,
                                progressStatus = ProgressStatus.SimpleProgress,
                                Remark = "From Case",
                                ActivityId = activity.Id,
                                CreatedBy = selectedHistory.CreatedBy,
                                EmployeeValueId = employee.Id,
                                CaseId = selectedHistory.Id,
                                Lat = "",
                                Lng = "",
                            };

                            _dbContext.ActivityProgresses.Add(activityProgress2);
                            await _dbContext.SaveChangesAsync();
                        }
                    }
                }

                string name = currentCase.Applicant != null ? currentCase.Applicant.ApplicantName : currentCase.Employee.FullName;
                string message = name + "\nበጉዳይ ቁጥር፡" + currentCase.CaseNumber + "\nየተመዘገበ ጉዳዮ በ፡" + currentHist.ToStructure.StructureName + " ተጠናቋል\nየቢሮ ቁጥር: - ";

                //await _smshelper.SendSmsForCase(message, currentHist.CaseId, currentHist.Id, UserId.ToString(), MessageFrom.Complete);
                response.Message = "Operation Successfull";
                response.Data = 1;
                response.Success = true;
                _logger.LogUpdate("CaseProcessingService", caseCompleteDto.CaseHistoryId.ToString(), "Case Completed.");
                return response;

            }
            catch (Exception ex)
            {
                response.Message = $"{ex.Message}";
                response.Data = 0;
                response.ErrorCode = HttpStatusCode.InternalServerError.ToString();
                response.Success = false;

                return response;
            }
        }
        public async Task<ResponseMessage<int>> RevertTask(CaseRevertDto revertCase)
        {
            var response = new ResponseMessage<int>();
            try
            {

                
                Employee? currEmp = await _dbContext.Employees.Include(x => x.OrganizationalStructure).Where(x => x.Id.Equals(revertCase.EmployeeId)).FirstOrDefaultAsync();
                
                if (currEmp == null){

                    response.Message = "Current Employee doesnt exist.";
                    response.Data = 0;
                    response.ErrorCode = HttpStatusCode.NotFound.ToString();
                    response.Success = false;

                    return response;
                }
                CaseHistory? selectedHistory = _dbContext.CaseHistories.Find(revertCase.CaseHistoryId);
                Guid UserId = Guid.Parse((await _userManager.Users.Where(appUsr => appUsr.EmployeesId.Equals(selectedHistory.ToEmployeeId)).FirstAsync()).Id);

                selectedHistory.AffairHistoryStatus = AffairHistoryStatus.Revert;
                selectedHistory.RevertedAt = DateTime.Now;
                selectedHistory.Remark = revertCase.Remark;

                _dbContext.CaseHistories.Attach(selectedHistory);
                _dbContext.Entry(selectedHistory).Property(x => x.AffairHistoryStatus).IsModified = true;
                _dbContext.Entry(selectedHistory).Property(x => x.RevertedAt).IsModified = true;
                _dbContext.Entry(selectedHistory).Property(x => x.Remark).IsModified = true;
                CaseHistory newHistory = new()
                {
                    Id = Guid.NewGuid(),
                    CreatedAt = DateTime.Now,
                    CreatedBy = Guid.Parse(_userManager.Users.Where(appUsr => appUsr.EmployeesId.Equals(revertCase.EmployeeId)).First().Id),
                    RowStatus = RowStatus.Active,
                    FromEmployeeId = revertCase.EmployeeId,
                    FromStructureId = currEmp.OrganizationalStructureId,
                    ToEmployeeId = selectedHistory.ToEmployeeId,
                    ToStructureId = selectedHistory.ToStructureId,
                    Remark = "",
                    CaseId = selectedHistory.CaseId,
                    ReciverType = ReciverType.Orginal,
                    childOrder = selectedHistory.childOrder += 1,
                };


                await _dbContext.CaseHistories.AddAsync(newHistory);
                await _dbContext.SaveChangesAsync();

                Case? currentCase = await _dbContext.Cases.Include(x => x.Applicant).Include(x => x.Employee).FirstOrDefaultAsync(x => x.Id == selectedHistory.CaseId);
                if (currentCase == null){

                    response.Message = "Selected Case is Empty";
                    response.Data = 0;
                    response.ErrorCode = HttpStatusCode.NotFound.ToString();
                    response.Success = false;

                    return response;
                }
                
                string name = currentCase.Applicant != null ? currentCase.Applicant.ApplicantName : currentCase.Employee.FullName;
                var message = name + "\nበጉዳይ ቁጥር፡" + currentCase.CaseNumber + "\nየተመዘገበ ጉዳዮ በ፡" + selectedHistory.ToStructure.StructureName + " ወደኋላ ተመልሷል  \nየቢሮ ቁጥር: -";

                //await _smshelper.SendSmsForCase(message, newHistory.CaseId, newHistory.Id, UserId.ToString(), MessageFrom.Revert);
                response.Data = 1;
                response.Success = true;
                response.Message = "Opertation Succesfull";
                _logger.LogUpdate("CaseProcessingService", revertCase.CaseHistoryId.ToString(), "Case reverted.");
                return response;
            }
            catch (Exception ex)
            {
                response.Message = $"{ex.Message}";
                response.Data = 0;
                response.ErrorCode = HttpStatusCode.InternalServerError.ToString();
                response.Success = false;

                return response;
            }
        }

        public async Task<ResponseMessage<int>> TransferCase(CaseTransferDto caseTransferDto)
        {
            var response = new ResponseMessage<int>();
            
            try
            {
                Employee? currEmp = await _dbContext.Employees.Where(el => el.Id.Equals(caseTransferDto.FromEmployeeId)).FirstOrDefaultAsync();
                if (currEmp == null){

                    response.Message = "Selected Employee doesnt exist.";
                    response.Data = 0;
                    response.ErrorCode = HttpStatusCode.NotFound.ToString();
                    response.Success = false;

                    return response;
                }
                CaseHistory currentLastHistory = await _dbContext.CaseHistories.Where(el => el.Id.Equals(caseTransferDto.CaseHistoryId)).OrderByDescending(x => x.CreatedAt).FirstAsync();

                Guid UserId = Guid.Parse((await _userManager.Users.Where(appUsr => appUsr.EmployeesId.Equals(caseTransferDto.ToEmployeeId)).FirstAsync()).Id);

                if (caseTransferDto.FromEmployeeId != currentLastHistory.ToEmployeeId)
                {

                    response.Message = "You are not authorized to transfer this case.";
                    response.Data = 0;
                    response.ErrorCode = HttpStatusCode.MethodNotAllowed.ToString();
                    response.Success = false;

                    return response;

                }

                currentLastHistory.AffairHistoryStatus = AffairHistoryStatus.Transfered;
                currentLastHistory.TransferedDateTime = DateTime.Now;

                _dbContext.CaseHistories.Attach(currentLastHistory);
                _dbContext.Entry(currentLastHistory).Property(c => c.AffairHistoryStatus).IsModified = true;
                _dbContext.Entry(currentLastHistory).Property(c => c.TransferedDateTime).IsModified = true;

                var toEmployee = caseTransferDto.ToEmployeeId == Guid.Empty || caseTransferDto.ToEmployeeId == null ?
                   _dbContext.Employees.FirstOrDefault(
                       e =>
                           e.OrganizationalStructureId == caseTransferDto.ToStructureId &&
                           e.Position == Position.Director).Id : caseTransferDto.ToEmployeeId;


                var newHistory = new CaseHistory
                {
                    Id = Guid.NewGuid(),
                    CreatedAt = DateTime.Now,
                    CreatedBy = UserId,

                    RowStatus = RowStatus.Active,
                    FromEmployeeId = caseTransferDto.FromEmployeeId,
                    FromStructureId = currEmp.OrganizationalStructureId,
                    ToEmployeeId = toEmployee,
                    ToStructureId = caseTransferDto.ToStructureId,
                    Remark = caseTransferDto.Remark,
                    CaseId = currentLastHistory.CaseId,
                    ReciverType = ReciverType.Orginal,
                    CaseTypeId = currentLastHistory.CaseTypeId,
                    childOrder = currentLastHistory.childOrder + 1
                    //must be change
                };


                await _dbContext.CaseHistories.AddAsync(newHistory);
                await _dbContext.CaseAttachments.AddRangeAsync(caseTransferDto.CaseAttachments);
                await _dbContext.SaveChangesAsync();


                /// Sending SMS
                Case? currentCase = await _dbContext.Cases.Include(x => x.Applicant).Include(x => x.Employee).FirstOrDefaultAsync(x => x.Id == newHistory.CaseId);
                if (currentCase == null){

                    response.Message = "Selected Case is Empty";
                    response.Data = 0;
                    response.ErrorCode = HttpStatusCode.NotFound.ToString();
                    response.Success = false;

                    return response;
                }
                string name = currentCase.Applicant != null ? currentCase.Applicant.ApplicantName : currentCase.Employee.FullName;
                string toStructure = _dbContext.OrganizationalStructures.Find(newHistory.ToStructureId).StructureName;


                var assigndCase = await _caseEncodeService.GetAllTransfred(toEmployee);
                await _encoderHub.Clients.Group(toEmployee.ToString()).getNotification(assigndCase.Data, toEmployee.ToString());

                string message = name + "\nበጉዳይ ቁጥር፡" + currentCase.CaseNumber + "\nየተመዘገበ ጉዳዮ ለ " + toStructure + " ተላልፏል\nየቢሮ ቁጥር:";

                //await _smshelper.SendSmsForCase(message, newHistory.CaseId, newHistory.Id, UserId.ToString(), MessageFrom.Transfer);
                response.Message = "Operation Successfull.";
                response.Data = 1;
                response.Success = true;
                _logger.LogCreate("CaseProcessingService", caseTransferDto.FromEmployeeId.ToString(), $"case transfered to {caseTransferDto.ToEmployeeId} Successfully");
                return response;
            }
            catch (Exception ex)

            {
                response.Message = $"{ex.Message}";
                response.Data = 0;
                response.ErrorCode = HttpStatusCode.InternalServerError.ToString();
                response.Success = false;

                return response;
            }
        }



        public async Task<ResponseMessage<int>> AddToWaiting(Guid caseHistoryId)
        {
            var response = new ResponseMessage<int>();
            try
            {


                var history = _dbContext.CaseHistories.Find(caseHistoryId);
                if (history == null){

                    response.Message = "History doesnt exist.";
                    response.Data = 0;
                    response.ErrorCode = HttpStatusCode.NotFound.ToString();
                    response.Success = false;

                    return response;
                }
                if (history.WaitingPeriod > 3)
                {
                    
                    response.Data = 0;
                    response.Success = false;
                    response.Message = "Case Has Reached Maximum Waiting";
                    return response;
                }

                history.AffairHistoryStatus = AffairHistoryStatus.Waiting;
                history.SeenDateTime = null;
                history.WaitingPeriod++;
                _dbContext.CaseHistories.Attach(history);
                _dbContext.Entry(history).Property(c => c.AffairHistoryStatus).IsModified = true;
                _dbContext.Entry(history).Property(c => c.SeenDateTime).IsModified = true;
                _dbContext.Entry(history).Property(c => c.WaitingPeriod).IsModified = true;


                await _dbContext.SaveChangesAsync();

                response.Message = "Added Successfully";
                response.Data = 1;
                response.Success = true;
                _logger.LogCreate("CaseProcessingService", caseHistoryId.ToString(), "Case added to waiting Successfully");
                return response;



            }
            catch (Exception ex)
            {
                response.Message = $"{ex.Message}";
                response.Data = 0;
                response.ErrorCode = HttpStatusCode.InternalServerError.ToString();
                response.Success = false;

                return response;
            }
        }

        public async Task<ResponseMessage<int>> SendSMS(CaseCompleteDto smsdetail)
        {
            var response = new ResponseMessage<int>();

            var history = _dbContext.CaseHistories.Find(smsdetail.CaseHistoryId);
            await _smshelper.SendSmsForCase(smsdetail.Remark, history.CaseId, history.Id, smsdetail.EmployeeId.ToString(), MessageFrom.Custom_text);
            response.Message = "Successfull.";
            response.Success = true;
            response.Data = 1;
            
            return response;
        }

        public async Task<ResponseMessage<CaseEncodeGetDto>> GetCaseDetial(Guid employeeId, Guid historyId)
        {

            var response = new ResponseMessage<CaseEncodeGetDto>();
            try{

            Employee? user = _dbContext.Employees.Include(x => x.OrganizationalStructure).Where(x => x.Id == employeeId).FirstOrDefault();

            if (user ==  null){

                response.Message = "Employee doesnt exist.";
                response.Data = null;
                response.ErrorCode = HttpStatusCode.NotFound.ToString();
                response.Success = false;

                return response;
            }

            CaseHistory? currentHistry = _dbContext.CaseHistories
                .Include(x => x.Case.CaseType)
                 .Include(x => x.Case.Applicant)
                 .Include(x => x.Case.Employee)
                .Include(x => x.FromEmployee)
                .Include(x => x.FromStructure)
                .Include(x => x.Case)?.FirstOrDefault(x =>

                                     x.Id == historyId);


            if (currentHistry == null){

                response.Message = "Selected history deosnt exist.";
                response.Data = null;
                response.ErrorCode = HttpStatusCode.NotFound.ToString();
                response.Success = false;

                return response;
            }

            if (currentHistry != null && (currentHistry.AffairHistoryStatus == AffairHistoryStatus.Pend || currentHistry.AffairHistoryStatus == AffairHistoryStatus.Waiting || currentHistry.AffairHistoryStatus == AffairHistoryStatus.Completed))
            {
                currentHistry.AffairHistoryStatus = AffairHistoryStatus.Seen;
                currentHistry.SeenDateTime = DateTime.Now;
                _dbContext.CaseHistories.Attach(currentHistry);
                _dbContext.Entry(currentHistry).Property(x => x.AffairHistoryStatus).IsModified = true;
                _dbContext.Entry(currentHistry).Property(x => x.SeenDateTime).IsModified = true;
                _dbContext.SaveChanges();
            }



            List<SelectListDto> attachments = (from x in _dbContext.CaseAttachments.Where(x => x.CaseId == currentHistry.CaseId)
                                               select new SelectListDto
                                               {
                                                   Id = x.Id,
                                                   Name = x.FilePath


                                               }).ToList();

            attachments.AddRange((from x in _dbContext.FilesInformations.Where(x => x.CaseId == currentHistry.CaseId)
                             select new SelectListDto { Id = x.Id, Name = x.FilePath, Photo  = x.FileDescription }).ToList());

            List<CaseDetailStructureDto> caseDetailstructures = _dbContext.CaseHistories.Include(x => x.FromEmployee).Include(x => x.FromStructure).Where(x => x.CaseId == currentHistry.CaseId).OrderByDescending(x => x.CreatedAt).Select(x => new CaseDetailStructureDto
            {
                FromEmployee = x.FromEmployee.FullName,
                FormStructure = x.FromStructure.StructureName,
                SeenDate = x.CreatedAt.ToString()

            }).ToList();

            CaseEncodeGetDto result = new CaseEncodeGetDto
            {
                Id = currentHistry.Id,
                CaseTypeName = currentHistry.Case.CaseType.CaseTypeTitle,
                CaseNumber = currentHistry.Case.CaseNumber,
                CreatedAt = currentHistry.Case.CreatedAt.ToString(),
                ApplicantName = currentHistry.Case.ApplicantId != null ? currentHistry.Case.Applicant.ApplicantName : "",
                ApplicantPhoneNo = currentHistry.Case.ApplicantId != null ? currentHistry.Case.Applicant.PhoneNumber : "",
                EmployeeName = currentHistry.Case.EmployeeId != null ? currentHistry.Case.Employee?.FullName : "",
                EmployeePhoneNo = currentHistry.Case.EmployeeId != null ? currentHistry.Case.Employee?.PhoneNumber : "",
                //ApplicantName = currentHistry.Case.Applicant.ApplicantName,
                //ApplicantPhoneNo = currentHistry.Case.Applicant.PhoneNumber,
                //EmployeeName = currentHistry.Case.Employee?.FullName,
                //EmployeePhoneNo = currentHistry.Case.Employee?.PhoneNumber,
                LetterNumber = currentHistry.Case.LetterNumber,
                LetterSubject = currentHistry.Case.LetterSubject,
                Position = user.Position.ToString(),
                FromStructure = currentHistry.FromStructure.StructureName,
                FromEmployeeId = currentHistry.FromEmployee.FullName,
                ReciverType = currentHistry.ReciverType.ToString(),
                SecreateryNeeded = currentHistry.SecreateryNeeded,
                IsConfirmedBySeretery = currentHistry.IsConfirmedBySeretery,
                ToEmployee = currentHistry.ToEmployee?.FullName,
                ToStructure = currentHistry.ToStructure?.StructureName,
                AffairHistoryStatus = currentHistry.AffairHistoryStatus.ToString(),
                Attachments = attachments,
                CaseTypeId = currentHistry.Case.CaseTypeId.ToString(),
                CaseDetailStructures = caseDetailstructures

            };

            response.Message = "Operation Successfull";
            response.Data = result;
            response.Success = true;

            return response;

            }
            catch (Exception ex){

                response.Message = $"{ex.Message}";
                response.Data = null;
                response.ErrorCode = HttpStatusCode.InternalServerError.ToString();
                response.Success = false;

                return response;
            }







        }


        public async Task<ResponseMessage<int>> ArchiveCase(ArchivedCaseDto archivedCaseDto)
        {

            var response = new ResponseMessage<int>();
            
            try{

                Case? cases = _dbContext.Cases.Find(archivedCaseDto.CaseId);

                if (cases == null){

                    response.Message = "Could not find matching cases.";
                    response.Data = 0;
                    response.ErrorCode = HttpStatusCode.NotFound.ToString();
                    response.Success = false;

                    return response;
                }

                cases.FolderId = archivedCaseDto.FolderId;
                cases.IsArchived = true;


                _dbContext.Entry(cases).Property(x => x.FolderId).IsModified = true;
                _dbContext.Entry(cases).Property(x => x.IsArchived).IsModified = true;

                await _dbContext.SaveChangesAsync();

                response.Message = "Operation Successfull";
                response.Data = 1;
                response.Success = true;
                    
                _logger.LogUpdate("CaseProcessingService", archivedCaseDto.CaseId.ToString(), "Case added to archive Successfully");
                return response;

            }

            catch (Exception ex){
                
                response.Message = $"{ex.Message}";
                response.Data = 0;
                response.Success = false;
                response.ErrorCode = HttpStatusCode.InternalServerError.ToString();

                return response;

            }
        }


        public async Task<ResponseMessage<CaseState>> GetCaseState(Guid CaseTypeId, Guid caseHistoryId)
        {
            var response = new ResponseMessage<CaseState>();

            try{

                var childCaseType = await _dbContext.CaseTypes.Where(x => x.ParentCaseTypeId == CaseTypeId).ToListAsync();
                CaseState caseState = new CaseState();

                var child_nullable = _dbContext.CaseHistories.Find(caseHistoryId);

                if (child_nullable == null){

                    response.Message = "Could not find child.";
                    response.Data = null;
                    response.ErrorCode = HttpStatusCode.NotFound.ToString();
                    response.Success = false;

                    return response;
                }

                int childcount = child_nullable.childOrder + 1;

                foreach (var childaffair in childCaseType)
                {
                

              
                    if (childaffair.OrderNumber == childcount)
                    {
                        caseState.CurrentState = childaffair.CaseTypeTitle;

                        caseState.NeededDocuments = new List<string>();

                        var files = await _dbContext.FileSettings.Where(x => x.CaseTypeId == childaffair.Id).ToListAsync();

                        foreach (var file in files)
                        {
                            caseState.NeededDocuments.Add(file.FileName);
                        }
                    }

                    if (childaffair.OrderNumber == childcount + 1)
                    {

                        caseState.NextState = childaffair.CaseTypeTitle;
                    }
                }

                response.Message = "Opertaion Successfull.";
                response.Data = caseState;
                response.Success = true;

                return response;
            
            }

            catch (Exception ex){

                response.Message = $"{ex.Message}";
                response.Data = null;
                response.ErrorCode = HttpStatusCode.InternalServerError.ToString();
                response.Success = false;

                return response;
            }
        }

        public async Task<ResponseMessage<bool>> Ispermitted(Guid employeeId, Guid caseId)
        {
            var response = new ResponseMessage<bool>();
            
            try{
            var caseIDD_nullable = _dbContext.CaseHistories.Find(caseId);

            if (caseIDD_nullable == null){
                
                response.Message = "Could not find matching case.";
                response.Data = false;
                response.ErrorCode = HttpStatusCode.NotFound.ToString();
                response.Success = false;

                return response;
            }
            
            var caseIDD = caseIDD_nullable.CaseId;

            var employee_nullable = _dbContext.CaseHistories.Where(x => x.CaseId == caseIDD && x.ReciverType == ReciverType.Orginal).OrderByDescending(z => z.childOrder).FirstOrDefault();

            if (employee_nullable == null){
                
                response.Message = "Could not find matching employee.";
                response.Data = false;
                response.ErrorCode = HttpStatusCode.NotFound.ToString();
                response.Success = false;

                return response;
            }

            var employee = employee_nullable.ToEmployeeId;

            var casehistor = _dbContext.CaseHistories.Where(x => x.CaseId == caseIDD).OrderBy(x => x.childOrder).Where(x => x.CompletedDateTime != null).Select(x => x.CompletedDateTime).ToList();

            if ((employeeId.ToString().ToLower() == employee.ToString().ToLower()) && casehistor.Count == 0)
            {
                response.Message = "Is permitted";
                response.Data = true;
                response.Success = true;

                return response;
            }

            response.Message = "Is not permitted";
            response.Data = false;
            response.Success = true;

            return response; 
            
            }

            catch (Exception ex){

                response.Message = $"{ex.Message}";
                response.Data = false;
                response.ErrorCode = HttpStatusCode.InternalServerError.ToString();
                response.Success = false;

                return response;
            }

        }


    }



    public class CaseState
    {
        public string CurrentState { get; set; }
        public string NextState { get; set; }
        public List<string> NeededDocuments { get; set; }
    }
}

