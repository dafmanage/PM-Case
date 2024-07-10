using Microsoft.EntityFrameworkCore;
using PM_Case_Managemnt_Implementation.DTOS.Common;
using PM_Case_Managemnt_Implementation.DTOS.PM;
using PM_Case_Managemnt_Implementation.Helpers;
using PM_Case_Managemnt_Implementation.Helpers.Logger;
using PM_Case_Managemnt_Implementation.Helpers.Response;
using PM_Case_Managemnt_Infrustructure.Data;
using PM_Case_Managemnt_Infrustructure.Models.PM;

namespace PM_Case_Managemnt_Implementation.Services.PM
{
    public class TaskService : ITaskService
    {

        private readonly ApplicationDbContext _dBContext;
        private readonly ILoggerManagerService _logger;
        public TaskService(ApplicationDbContext context, ILoggerManagerService logger)
        {
            _dBContext = context;
            _logger = logger;
        }

        public async Task<ResponseMessage<int>> CreateTask(TaskDto task)
        {
            var response = new ResponseMessage<int>();

            var task1 = new PM_Case_Managemnt_Infrustructure.Models.PM.Task
            {
                Id = Guid.NewGuid(),
                TaskDescription = task.TaskDescription,
                PlanedBudget = task.PlannedBudget,
                HasActivityParent = task.HasActvity,
                CreatedAt = DateTime.Now,
                PlanId = task.PlanId,

            };

            await _dBContext.AddAsync(task1);
            await _dBContext.SaveChangesAsync();
            response.Message = "Operation Successful.";
            response.Success = true;
            response.Data = 1;
            _logger.LogCreate("TaskService", task.Id.ToString(), "Task Created Successfully");
            return response;

        }

        public async Task<ResponseMessage<int>> AddTaskMemo(TaskMemoRequestDto taskMemo)
        {
            var response = new ResponseMessage<int>();
            var taskMemo1 = new TaskMemo
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.Now,
                EmployeeId = taskMemo.EmployeeId,
                Description = taskMemo.Description,
            };

            if (taskMemo.RequestFrom == "PLAN")
            {
                taskMemo1.PlanId = taskMemo.TaskId;
            }

            else
            {
                taskMemo1.TaskId = taskMemo.TaskId;
            }

            await _dBContext.AddAsync(taskMemo1);
            await _dBContext.SaveChangesAsync();
            response.Message = "Operation Successful.";
            response.Success = true;
            response.Data = 1;
            _logger.LogCreate("TaskService", taskMemo.TaskId.ToString(), "Task Memo Added Successfully");
            return response;
        }

        public async Task<ResponseMessage<TaskVIewDto>> GetSingleTask(Guid taskId)
        {
            var response = new ResponseMessage<TaskVIewDto>();

            var task = await _dBContext.Tasks.Include(x => x.Plan).FirstOrDefaultAsync(x => x.Id == taskId);

            if (task != null)
            {

                var taskMembers = (from t in _dBContext.Employees.Where(x => x.OrganizationalStructureId == task.Plan.StructureId)
                                   select new SelectListDto
                                   {
                                       Name = t.FullName,
                                       Photo = t.Photo,
                                       EmployeeId = t.Id.ToString()
                                   }).ToList();


                var taskMemos = (from t in _dBContext.TaskMemos.Include(x => x.Employee).Where(x => x.TaskId == taskId)
                                 select new TaskMemoDto
                                 {
                                     Employee = new SelectListDto
                                     {
                                         Id = t.EmployeeId,
                                         Name = t.Employee.FullName,
                                         Photo = t.Employee.Photo,
                                     },
                                     DateTime = t.CreatedAt,
                                     Description = t.Description

                                 }).ToList();


                var activityProgress = _dBContext.ActivityProgresses;

                var activityViewDtos = new List<ActivityViewDto>();


                activityViewDtos.AddRange([.. (from e in _dBContext.ActivityParents.Include(x => x.UnitOfMeasurment).Where(x => x.TaskId == taskId)
                                           select new ActivityViewDto
                                           {
                                               Id = e.Id,
                                               Name = e.ActivityParentDescription,
                                               PlannedBudget = e.PlanedBudget,
                                               AssignedToBranch = e.AssignedToBranch,
                                               Weight = e.Weight,
                                               Begining = e.BaseLine,
                                               Target = e.Goal,
                                               UnitOfMeasurment = e.UnitOfMeasurment.Name,
                                               UnitOfMeasurmentId = e.UnitOfMeasurmentId,
                                               OverAllPerformance = 0,
                                               StartDate = e.ShouldStartPeriod.ToString(),
                                               EndDate = e.ShouldEnd.ToString(),
                                               IsClassfiedToBranch = e.IsClassfiedToBranch,
                                               Members = new List<SelectListDto>(),
                                               MonthPerformance = new List<MonthPerformanceViewDto>(),
                                               OverAllProgress = 0,
                                               UsedBudget = activityProgress.Where(x => x.ActivityId == e.Id && x.IsApprovedByFinance == ApprovalStatus.Approved).Sum(x => x.ActualBudget),

                                               StartDateEth = e.ShouldStartPeriod != null
                                                    ? EthiopicDateTime.GetEthiopicDateUS(e.ShouldStartPeriod.Value.Day, e.ShouldStartPeriod.Value.Month, e.ShouldStartPeriod.Value.Year)
                                                     : null,
                                               EndDateEth = e.ShouldEnd != null
                                                ? EthiopicDateTime.GetEthiopicDateUS(e.ShouldEnd.Value.Day, e.ShouldEnd.Value.Month, e.ShouldEnd.Value.Year)
                                                : null,
                                               OfficeWork = 0,
                                               FieldWork = 0,

                                           }
                                        )]);


                activityViewDtos.AddRange([.. (from e in _dBContext.Activities.Include(x => x.UnitOfMeasurement)
                                           where e.TaskId == task.Id
        
                                           select new ActivityViewDto
                                           {
                                               Id = e.Id,
                                               Name = e.ActivityDescription,
                                               PlannedBudget = e.PlanedBudget,
                                               ActivityType = e.ActivityType.ToString(),
                                               Weight = e.Weight,
                                               Begining = e.Begining,
                                               Target = e.Goal,
                                               UnitOfMeasurment = e.UnitOfMeasurement.Name,
                                               UnitOfMeasurmentId = e.UnitOfMeasurementId,
                                               OverAllPerformance = 0,
                                               HasKpiGoal = e.HasKpiGoal,
                                               KpiGoalId = e.KpiGoalId,
                                               StartDate = e.ShouldStart.ToString(),
                                               EndDate = e.ShouldEnd.ToString(),
                                               IsClassfiedToBranch = false,
                                               BranchId = e.OrganizationalStructureId != null ? e.OrganizationalStructureId : null,
                                               Members = e.CommiteeId == null ? _dBContext.EmployeesAssignedForActivities.Include(x => x.Employee).Where(x => x.ActivityId == e.Id).Select(y => new SelectListDto
                                               {
                                                   Id = y.Id,
                                                   Name = y.Employee.FullName,
                                                   Photo = y.Employee.Photo,
                                                   EmployeeId = y.EmployeeId.ToString(),

                                               }).ToList() : _dBContext.CommiteEmployees.Where(x => x.CommiteeId == e.CommiteeId).Include(x => x.Employee)
                                               .Select(y => new SelectListDto
                                               {
                                                   Id = y.Id,
                                                   Name = y.Employee.FullName,
                                                   Photo = y.Employee.Photo,
                                                   EmployeeId = y.EmployeeId.ToString(),

                                               }).ToList(),
                                               MonthPerformance = _dBContext.ActivityTargetDivisions.Where(x => x.ActivityId == e.Id).OrderBy(x => x.Order).Select(y => new MonthPerformanceViewDto
                                               {
                                                   Id = y.Id,
                                                   Order = y.Order,
                                                   Planned = y.Target,
                                                   Actual = activityProgress.Where(x => x.QuarterId == y.Id).Sum(x => x.ActualWorked),
                                                   Percentage = y.Target != 0 ? activityProgress.Where(x => x.QuarterId == y.Id && x.IsApprovedByDirector == ApprovalStatus.Approved && x.IsApprovedByFinance == ApprovalStatus.Approved && x.IsApprovedByManager == ApprovalStatus.Approved).Sum(x => x.ActualWorked) / y.Target * 100 : 0

                                               }).ToList(),
                                               OverAllProgress = e.Goal != 0 ? activityProgress.Where(x => x.ActivityId == e.Id && x.IsApprovedByDirector == ApprovalStatus.Approved && x.IsApprovedByFinance == ApprovalStatus.Approved && x.IsApprovedByManager == ApprovalStatus.Approved).Sum(x => x.ActualWorked) * 100 / e.Goal : 0,
                                               UsedBudget = activityProgress.Where(x => x.ActivityId == e.Id && x.IsApprovedByFinance == ApprovalStatus.Approved).Sum(x => x.ActualBudget),
                                               OfficeWork = e.OfficeWork,
                                               FieldWork = e.FieldWork,
                                               CommiteeId = e.CommiteeId,
                                               StartDateEth = e.ShouldStart != null
                                                       ? EthiopicDateTime.GetEthiopicDateUS(e.ShouldStart.Day, e.ShouldStart.Month, e.ShouldStart.Year)
                                                        : null,
                                               EndDateEth = e.ShouldEnd != null
                                                   ? EthiopicDateTime.GetEthiopicDateUS(e.ShouldEnd.Day, e.ShouldEnd.Month, e.ShouldEnd.Year)
                                                   : null,



                                           }
                                          )]);





                var result = new TaskVIewDto
                {

                    Id = task.Id,
                    TaskName = task.TaskDescription,
                    TaskMembers = taskMembers,
                    TaskMemos = taskMemos,
                    PlannedBudget = task.PlanedBudget,
                    RemainingBudget = task.PlanedBudget - activityViewDtos.Sum(x => x.PlannedBudget),
                    ActivityViewDtos = activityViewDtos,
                    TaskWeight = activityViewDtos.Sum(x => x.Weight),
                    RemainingWeight = 100 - activityViewDtos.Sum(x => x.Weight),
                    NumberofActivities = _dBContext.Activities.Include(x => x.ActivityParent).Count(x => x.TaskId == task.Id || x.ActivityParent.TaskId == task.Id)
                };
                response.Message = "Operation Successful.";
                response.Success = true;
                response.Data = result;

                return response;
            }
            else
            {
                var plan = await _dBContext.Plans.FirstOrDefaultAsync(x => x.Id == taskId);

                if (plan != null)
                {


                    var taskMembers = (from t in _dBContext.Employees.Where(x => x.OrganizationalStructureId == plan.StructureId)
                                       select new SelectListDto
                                       {
                                           Id = t.Id,
                                           Name = t.FullName,
                                           Photo = t.Photo,
                                           EmployeeId = t.Id.ToString()
                                       }).ToList();


                    var taskMemos = (from t in _dBContext.TaskMemos.Include(x => x.Employee).Where(x => x.PlanId == plan.Id)
                                     select new TaskMemoDto
                                     {
                                         Employee = new SelectListDto
                                         {
                                             Id = t.EmployeeId,
                                             Name = t.Employee.FullName,
                                             Photo = t.Employee.Photo,
                                         },
                                         DateTime = t.CreatedAt,
                                         Description = t.Description

                                     }).ToList();


                    var activityProgress = _dBContext.ActivityProgresses;

                    var activityViewDtos = (from e in _dBContext.Activities.Include(x => x.UnitOfMeasurement)
                                            where e.PlanId == plan.Id
                                            select new ActivityViewDto
                                            {
                                                Id = e.Id,
                                                Name = e.ActivityDescription,
                                                PlannedBudget = e.PlanedBudget,
                                                ActivityType = e.ActivityType.ToString(),
                                                Weight = e.Weight,
                                                Begining = e.Begining,
                                                Target = e.Goal,
                                                UnitOfMeasurment = e.UnitOfMeasurement.Name,
                                                OverAllPerformance = 0,
                                                HasKpiGoal = e.HasKpiGoal,
                                                KpiGoalId = e.KpiGoalId,
                                                StartDate = e.ShouldStart.ToString(),
                                                EndDate = e.ShouldEnd.ToString(),
                                                BranchId = e.OrganizationalStructureId != null ? e.OrganizationalStructureId : null,
                                                Members = _dBContext.EmployeesAssignedForActivities.Include(x => x.Employee).Where(x => x.ActivityId == e.Id).Select(y => new SelectListDto
                                                {
                                                    Id = y.Id,
                                                    Name = y.Employee.FullName,
                                                    Photo = y.Employee.Photo,
                                                    EmployeeId = y.EmployeeId.ToString(),

                                                }).ToList(),
                                                MonthPerformance = _dBContext.ActivityTargetDivisions.Where(x => x.ActivityId == e.Id).OrderBy(x => x.Order).Select(y => new MonthPerformanceViewDto
                                                {
                                                    Id = y.Id,
                                                    Order = y.Order,
                                                    Planned = y.Target,
                                                    Actual = activityProgress.Where(x => x.QuarterId == y.Id).Sum(x => x.ActualWorked),
                                                    Percentage = y.Target != 0 ? activityProgress.Where(x => x.QuarterId == y.Id && x.IsApprovedByDirector == ApprovalStatus.Approved && x.IsApprovedByFinance == ApprovalStatus.Approved && x.IsApprovedByManager == ApprovalStatus.Approved).Sum(x => x.ActualWorked) / y.Target * 100 : 0

                                                }).ToList(),
                                                OverAllProgress = e.Goal != 0 ? activityProgress.Where(x => x.ActivityId == e.Id && x.IsApprovedByDirector == ApprovalStatus.Approved && x.IsApprovedByFinance == ApprovalStatus.Approved && x.IsApprovedByManager == ApprovalStatus.Approved).Sum(x => x.ActualWorked) * 100 / e.Goal : 0,
                                                UsedBudget = activityProgress.Where(x => x.ActivityId == e.Id && x.IsApprovedByFinance == ApprovalStatus.Approved).Sum(x => x.ActualBudget)

                                            }
                                            ).ToList();

                    var result =  new TaskVIewDto
                    {

                        Id = plan.Id,
                        TaskName = plan.PlanName,
                        TaskMembers = taskMembers,
                        TaskMemos = taskMemos,
                        PlannedBudget = plan.PlandBudget,
                        RemainingBudget = plan.PlandBudget - activityViewDtos.Sum(x => x.PlannedBudget),
                        ActivityViewDtos = activityViewDtos,
                        TaskWeight = activityViewDtos.Sum(x => x.Weight),
                        RemainingWeight = 100 - activityViewDtos.Sum(x => x.Weight),
                        NumberofActivities = activityViewDtos.Count(),

                    };
                    response.Message = "Operation Successful.";
                    response.Success = true;
                    response.Data = result;

                    return response;
                }

            }
            response.Message = "Operation Successful.";
            response.Success = true;
            response.Data = new TaskVIewDto();

            return response;

        }

        public async Task<ResponseMessage<List<ActivityViewDto>>> GetSingleActivityParent(Guid actParentId)
        {
            var response = new ResponseMessage<List<ActivityViewDto>>();
            var activityViewDtos = new List<ActivityViewDto>();
            var activityProgress = _dBContext.ActivityProgresses.Where(x => x.Activity.ActivityParentId == actParentId);
            activityViewDtos.AddRange([.. (from e in _dBContext.Activities.Include(x => x.UnitOfMeasurement)
                                       where e.ActivityParentId == actParentId
                                       // join ae in _dBContext.EmployeesAssignedForActivities.Include(x=>x.Employee) on e.Id equals ae.ActivityId
                                       select new ActivityViewDto
                                       {
                                           Id = e.Id,
                                           Name = e.ActivityDescription,
                                           PlannedBudget = e.PlanedBudget,
                                           ActivityType = e.ActivityType.ToString(),
                                           Weight = e.Weight,
                                           Begining = e.Begining,
                                           Target = e.Goal,
                                           UnitOfMeasurment = e.UnitOfMeasurement.Name,
                                           UnitOfMeasurmentId = e.UnitOfMeasurementId,
                                           BranchId = e.OrganizationalStructureId != null ? e.OrganizationalStructureId : null,
                                           OverAllPerformance = 0,
                                           HasKpiGoal = e.HasKpiGoal,
                                           KpiGoalId = e.KpiGoalId,
                                           StartDate = e.ShouldStart.ToString(),
                                           EndDate = e.ShouldEnd.ToString(),
                                           IsClassfiedToBranch = false,
                                           Members = e.CommiteeId == null ? _dBContext.EmployeesAssignedForActivities.Include(x => x.Employee).Where(x => x.ActivityId == e.Id).Select(y => new SelectListDto
                                           {
                                               Id = y.Id,
                                               Name = y.Employee.FullName,
                                               Photo = y.Employee.Photo,
                                               EmployeeId = y.EmployeeId.ToString(),
                                           }).ToList() : _dBContext.CommiteEmployees.Where(x => x.CommiteeId == e.CommiteeId).Include(x => x.Employee)
                                       .Select(y => new SelectListDto
                                       {
                                           Id = y.Id,
                                           Name = y.Employee.FullName,
                                           Photo = y.Employee.Photo,
                                           EmployeeId = y.EmployeeId.ToString(),
                                       }).ToList(),
                                           MonthPerformance = _dBContext.ActivityTargetDivisions.Where(x => x.ActivityId == e.Id).OrderBy(x => x.Order).Select(y => new MonthPerformanceViewDto
                                           {
                                               Id = y.Id,
                                               Order = y.Order,
                                               Planned = y.Target,
                                               Actual = activityProgress.Where(x => x.QuarterId == y.Id).Sum(x => x.ActualWorked),
                                               Percentage = y.Target != 0 ? activityProgress.Where(x => x.QuarterId == y.Id && x.IsApprovedByDirector == ApprovalStatus.Approved && x.IsApprovedByFinance == ApprovalStatus.Approved && x.IsApprovedByManager == ApprovalStatus.Approved).Sum(x => x.ActualWorked) / y.Target * 100 : 0
                                           }).ToList(),
                                           OverAllProgress = e.Goal != 0 ? activityProgress.Where(x => x.ActivityId == e.Id && x.IsApprovedByDirector == ApprovalStatus.Approved && x.IsApprovedByFinance == ApprovalStatus.Approved && x.IsApprovedByManager == ApprovalStatus.Approved).Sum(x => x.ActualWorked) * 100 / e.Goal : 0,


                                       }
                                            )]);

            response.Message = "Operation Successful.";
            response.Success = true;
            response.Data = activityViewDtos;

            return response;




        }
        public async Task<ResponseMessage<int>> AddTaskMemebers(TaskMembersDto taskMembers)
        {
            var response = new ResponseMessage<int>();
            if (taskMembers.RequestFrom == "PLAN")
            {
                foreach (var e in taskMembers.Employee)
                {
                    var taskMemebers1 = new TaskMembers
                    {
                        Id = Guid.NewGuid(),
                        CreatedAt = DateTime.Now,
                        EmployeeId = e.Id,
                        PlanId = taskMembers.TaskId
                    };
                    await _dBContext.AddAsync(taskMemebers1);
                    await _dBContext.SaveChangesAsync();
                }
            }
            else
            {
                foreach (var e in taskMembers.Employee)
                {
                    var taskMemebers1 = new TaskMembers
                    {
                        Id = Guid.NewGuid(),
                        CreatedAt = DateTime.Now,
                        EmployeeId = e.Id,
                        TaskId = taskMembers.TaskId
                    };
                    await _dBContext.AddAsync(taskMemebers1);
                    await _dBContext.SaveChangesAsync();
                }
            }

            response.Message = "Operation Successful.";
            response.Success = true;
            response.Data = 1;
            _logger.LogCreate("TaskService", taskMembers.TaskId.ToString(), "Task Members Added Successfully");
            return response;
        }
        public async Task<ResponseMessage<List<SelectListDto>>> GetEmployeesNoTaskMembersSelectList(Guid taskId, Guid subOrgId)
        {
            var response = new ResponseMessage<List<SelectListDto>>();
            var taskMembers = _dBContext.TaskMembers.Where(x =>
            (x.TaskId != Guid.Empty && x.TaskId == taskId) ||
            (x.PlanId != Guid.Empty && x.PlanId == taskId) ||
            (x.ActivityParentId != Guid.Empty && x.ActivityParentId == taskId)
            ).Select(x => x.EmployeeId).ToList();

            var EmployeeSelectList = await (from e in _dBContext.Employees.Where(x => x.OrganizationalStructure.SubsidiaryOrganizationId == subOrgId)
                                            where !(taskMembers.Contains(e.Id))
                                            select new SelectListDto
                                            {
                                                Id = e.Id,
                                                Name = e.FullName
                                            }).ToListAsync();

            response.Message = "Operation Successful.";
            response.Success = true;
            response.Data = EmployeeSelectList;

            return response;
        }

        public async Task<ResponseMessage<List<SelectListDto>>> GetTasksSelectList(Guid PlanId)
        {

            var response = new ResponseMessage<List<SelectListDto>>();

            List<SelectListDto> result = await _dBContext.Tasks.Where(x => x.PlanId == PlanId).
                Select(x => new SelectListDto { Id = x.Id, Name = x.TaskDescription }).ToListAsync();
            response.Message = "Operation Successful.";
            response.Success = true;
            response.Data = result;

            return response;
        }


        public async Task<ResponseMessage<List<SelectListDto>>> GetActivitieParentsSelectList(Guid TaskId)
        {
            var response = new ResponseMessage<List<SelectListDto>>();
            List<SelectListDto> result =  await _dBContext.ActivityParents.Where(x => x.TaskId == TaskId).Select(x => new SelectListDto
            {
                Id = x.Id,
                Name = x.ActivityParentDescription
            }).ToListAsync();
            response.Message = "Operation Successful.";
            response.Success = true;
            response.Data = result;

            return response;
        }

        public async Task<ResponseMessage<List<SelectListDto>>> GetActivitiesSelectList(Guid? planId, Guid? taskId, Guid? actParentId)
        {

            var response = new ResponseMessage<List<SelectListDto>>();

            if (planId != null)
            {
                List<SelectListDto> result = await _dBContext.Activities.Where(x => x.PlanId == planId)
             .Select(x => new SelectListDto
             {
                 Id = x.Id,
                 Name = x.ActivityDescription
             }).ToListAsync();
                response.Message = "Operation Successful.";
                response.Success = true;
                response.Data = result;

                return response;

            }
            if (taskId != null)
            {
                List<SelectListDto>  result_null = await _dBContext.Activities.Where(x => x.TaskId == taskId)
             .Select(x => new SelectListDto
             {
                 Id = x.Id,
                 Name = x.ActivityDescription
             }).ToListAsync();
                response.Message = "Operation Successful.";
                response.Success = true;
                response.Data = result_null;

                return response;

            }
            List<SelectListDto> result_op = await _dBContext.Activities.Where(x => x.ActivityParentId == actParentId)
                .Select(x => new SelectListDto
                {
                    Id = x.Id,
                    Name = x.ActivityDescription
                }).ToListAsync();
            response.Message = "Operation Successful.";
            response.Success = true;
            response.Data = result_op;

            return response;
        }


        public async Task<ResponseMessage<int>> UpdateTask(TaskDto updateTask)
        {
            try
            {
                var task = await _dBContext.Tasks.FindAsync(updateTask.Id);

                if (task != null)
                {
                    task.TaskDescription = updateTask.TaskDescription;
                    task.PlanedBudget = updateTask.PlannedBudget;
                    task.HasActivityParent = updateTask.HasActvity;
                    task.PlanId = updateTask.PlanId;

                    await _dBContext.SaveChangesAsync();
                    _logger.LogUpdate("TaskService", updateTask.Id.ToString(), "Task Updated Successfully");
                    return new ResponseMessage
                    {
                        Success = true,
                        Message = "Task Updated Successfully"
                    };
                }
                return new ResponseMessage<int>
                {
                    Success = false,
                    Message = "Task Not Found"
                };
            }
            catch (Exception ex)
            {
                return new ResponseMessage<int>
                {
                    Success = false,
                    Message = ex.Message.ToString()
                };
            }
        }

        public async Task<ResponseMessage<int>> DeleteTask(Guid taskId)
        {
            var task = await _dBContext.Tasks.FindAsync(taskId);

            if (task != null)
            {

                var taskMemos = await _dBContext.TaskMemos.Where(x => x.TaskId == task.Id).ToListAsync();
                var taskMembers = await _dBContext.TaskMembers.Where(x => x.TaskId == task.Id).ToListAsync();

                if (taskMemos.Count != 0)
                {
                    _dBContext.TaskMemos.RemoveRange(taskMemos);
                    await _dBContext.SaveChangesAsync();
                }
                if (taskMembers.Count != 0)
                {
                    _dBContext.TaskMembers.RemoveRange(taskMembers);
                    await _dBContext.SaveChangesAsync();
                }

                var activityParents = await _dBContext.ActivityParents.Where(x => x.TaskId == task.Id).ToListAsync();

                if (activityParents.Count != 0)
                {
                    foreach (var actP in activityParents)
                    {
                        var actvities = await _dBContext.Activities.Where(x => x.ActivityParentId == actP.Id).ToListAsync();

                        foreach (var act in actvities)
                        {
                            var actProgress = await _dBContext.ActivityProgresses.Where(x => x.ActivityId == act.Id).ToListAsync();

                            foreach (var actpro in actProgress)
                            {
                                var progAttachments = await _dBContext.ProgressAttachments.Where(x => x.ActivityProgressId == actpro.Id).ToListAsync();
                                if (progAttachments.Count != 0)
                                {
                                    _dBContext.ProgressAttachments.RemoveRange(progAttachments);
                                    await _dBContext.SaveChangesAsync();
                                }

                            }

                            if (actProgress.Count != 0)
                            {
                                _dBContext.ActivityProgresses.RemoveRange(actProgress);
                                await _dBContext.SaveChangesAsync();
                            }

                            var activityTargets = await _dBContext.ActivityTargetDivisions.Where(x => x.ActivityId == act.Id).ToListAsync();


                            if (activityTargets.Count != 0)
                            {
                                _dBContext.ActivityTargetDivisions.RemoveRange(activityTargets);
                                await _dBContext.SaveChangesAsync();
                            }


                            var employees = await _dBContext.EmployeesAssignedForActivities.Where(x => x.ActivityId == act.Id).ToListAsync();


                            if (activityTargets.Count != 0)
                            {
                                _dBContext.EmployeesAssignedForActivities.RemoveRange(employees);
                                await _dBContext.SaveChangesAsync();
                            }





                        }
                    }

                    _dBContext.ActivityParents.RemoveRange(activityParents);
                    await _dBContext.SaveChangesAsync();

                }
                var actvities2 = await _dBContext.Activities.Where(x => x.TaskId == task.Id).ToListAsync();

                if (actvities2.Count != 0)
                {
                    foreach (var act in actvities2)
                    {
                        var actProgress = await _dBContext.ActivityProgresses.Where(x => x.ActivityId == act.Id).ToListAsync();

                        foreach (var actpro in actProgress)
                        {
                            var progAttachments = await _dBContext.ProgressAttachments.Where(x => x.ActivityProgressId == actpro.Id).ToListAsync();
                            if (progAttachments.Count != 0)
                            {
                                _dBContext.ProgressAttachments.RemoveRange(progAttachments);
                                await _dBContext.SaveChangesAsync();
                            }

                        }

                        if (actProgress.Count != 0)
                        {
                            _dBContext.ActivityProgresses.RemoveRange(actProgress);
                            await _dBContext.SaveChangesAsync();
                        }

                        var activityTargets = await _dBContext.ActivityTargetDivisions.Where(x => x.ActivityId == act.Id).ToListAsync();


                        if (activityTargets.Count != 0)
                        {
                            _dBContext.ActivityTargetDivisions.RemoveRange(activityTargets);
                            await _dBContext.SaveChangesAsync();
                        }


                        var employees = await _dBContext.EmployeesAssignedForActivities.Where(x => x.ActivityId == act.Id).ToListAsync();


                        if (employees.Count != 0)
                        {
                            _dBContext.EmployeesAssignedForActivities.RemoveRange(employees);
                            await _dBContext.SaveChangesAsync();
                        }

                        if (activityParents.Count != 0)
                        {
                            _dBContext.ActivityParents.RemoveRange(activityParents);
                            await _dBContext.SaveChangesAsync();
                        }



                    }

                    _dBContext.Activities.RemoveRange(actvities2);
                    await _dBContext.SaveChangesAsync();
                }


                _dBContext.Tasks.Remove(task);
                await _dBContext.SaveChangesAsync();
                _logger.LogUpdate("TaskService", taskId.ToString(), "Task Deleted Successfully");
                return new ResponseMessage
                {

                    Success = true,
                    Message = "Task Deleted Successfully !!!"

                };

            }
            return new ResponseMessage<int>
            {

                Success = false,
                Message = "Task Not Found !!!"

            };
        }
    }
}
