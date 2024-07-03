using Microsoft.EntityFrameworkCore;
using PM_Case_Managemnt_Implementation.DTOS.Common;
using PM_Case_Managemnt_Implementation.DTOS.PM;
using PM_Case_Managemnt_Implementation.Helpers;
using PM_Case_Managemnt_Infrustructure.Data;
using PM_Case_Managemnt_Infrustructure.Models.PM;

namespace PM_Case_Managemnt_Implementation.Services.PM
{
    public class TaskService : ITaskService
    {

        private readonly ApplicationDbContext _dBContext;
        public TaskService(ApplicationDbContext context)
        {
            _dBContext = context;
        }

        public async Task<int> CreateTask(TaskDto task)
        {

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
            return 1;

        }

        public async Task<int> AddTaskMemo(TaskMemoRequestDto taskMemo)
        {
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
            return 1;
        }

        public async Task<TaskViewDto> GetSingleTask(Guid taskId)
        {

            var task = await _dBContext.Tasks.Include(x => x.Plan).FirstOrDefaultAsync(x => x.Id == taskId);

            if (task != null)
            {

                //var taskMembers = (from t in _dBContext.TaskMembers.Include(x => x.Employee).Where(x => x.TaskId == task.Id)
                //                   select new SelectListDto
                //                   {
                //                       Id = t.Id,
                //                       Name = t.Employee.FullName,
                //                       Photo = t.Employee.Photo,
                //                       EmployeeId = t.EmployeeId.ToString()
                //                   }).ToList();

                var taskMembers = (from t in _dBContext.Employees.Where(x => x.OrganizationalStructureId == task.Plan.StructureId)
                                   select new SelectListDto
                                   {
                                       //Id = t.Id,
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


                activityViewDtos.AddRange((from e in _dBContext.ActivityParents.Include(x => x.UnitOfMeasurment).Where(x => x.TaskId == taskId)
                                           select new ActivityViewDto
                                           {
                                               Id = e.Id,
                                               Name = e.ActivityParentDescription,
                                               PlannedBudget = e.PlanedBudget,
                                               AssignedToBranch = e.AssignedToBranch,
                                               //ActivityType = e.ActivityType.ToString(),
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
                                               UsedBudget = activityProgress.Where(x => x.ActivityId == e.Id && x.IsApprovedByFinance == approvalStatus.approved).Sum(x => x.ActualBudget),

                                               StartDateEth = e.ShouldStartPeriod != null
                                                    ? EthiopicDateTime.GetEthiopicDateUS(e.ShouldStartPeriod.Value.Day, e.ShouldStartPeriod.Value.Month, e.ShouldStartPeriod.Value.Year)
                                                     : null,
                                               EndDateEth = e.ShouldEnd != null
                                                ? EthiopicDateTime.GetEthiopicDateUS(e.ShouldEnd.Value.Day, e.ShouldEnd.Value.Month, e.ShouldEnd.Value.Year)
                                                : null,
                                               OfficeWork = 0,
                                               FieldWork = 0,

                                           }
                                        ).ToList());


                activityViewDtos.AddRange((from e in _dBContext.Activities.Include(x => x.UnitOfMeasurement)
                                           where e.TaskId == task.Id
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
                                               OverAllPerformance = 0,
                                               HasKpiGoal = e.HasKpiGoal,
                                               KpiGoalId = e.KpiGoalId,
                                               StartDate = e.ShouldStat.ToString(),
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
                                                   Percentage = y.Target != 0 ? (activityProgress.Where(x => x.QuarterId == y.Id && x.IsApprovedByDirector == approvalStatus.approved && x.IsApprovedByFinance == approvalStatus.approved && x.IsApprovedByManager == approvalStatus.approved).Sum(x => x.ActualWorked) / y.Target) * 100 : 0

                                               }).ToList(),
                                               OverAllProgress = e.Goal != 0 ? activityProgress.Where(x => x.ActivityId == e.Id && x.IsApprovedByDirector == approvalStatus.approved && x.IsApprovedByFinance == approvalStatus.approved && x.IsApprovedByManager == approvalStatus.approved).Sum(x => x.ActualWorked) * 100 / e.Goal : 0,
                                               UsedBudget = activityProgress.Where(x => x.ActivityId == e.Id && x.IsApprovedByFinance == approvalStatus.approved).Sum(x => x.ActualBudget),
                                               OfficeWork = e.OfficeWork,
                                               FieldWork = e.FieldWork,
                                               CommiteeId = e.CommiteeId,
                                               StartDateEth = e.ShouldStat != null
                                                       ? EthiopicDateTime.GetEthiopicDateUS(e.ShouldStat.Day, e.ShouldStat.Month, e.ShouldStat.Year)
                                                        : null,
                                               EndDateEth = e.ShouldEnd != null
                                                   ? EthiopicDateTime.GetEthiopicDateUS(e.ShouldEnd.Day, e.ShouldEnd.Month, e.ShouldEnd.Year)
                                                   : null,



                                           }
                                          ).ToList());





                return new TaskVIewDto
                {

                    Id = task.Id,
                    TaskName = task.TaskDescription,
                    TaskMembers = taskMembers,
                    TaskMemos = taskMemos,
                    PlannedBudget = task.PlanedBudget,
                    RemainingBudget = task.PlanedBudget - activityViewDtos.Sum(x => x.PlannedBudget),
                    ActivityViewDtos = activityViewDtos,
                    TaskWeight = activityViewDtos.Sum(x => x.Weight),
                    RemianingWeight = 100 - activityViewDtos.Sum(x => x.Weight),
                    NumberofActivities = _dBContext.Activities.Include(x => x.ActivityParent).Count(x => x.TaskId == task.Id || x.ActivityParent.TaskId == task.Id)
                };
            }
            else
            {
                var plan = await _dBContext.Plans.FirstOrDefaultAsync(x => x.Id == taskId);

                if (plan != null)
                {
                    //var taskMembers = (from t in _dBContext.TaskMembers.Include(x => x.Employee).Where(x => x.PlanId == plan.Id)
                    //                   select new SelectListDto
                    //                   {
                    //                       Id = t.Id,
                    //                       Name = t.Employee.FullName,
                    //                       Photo = t.Employee.Photo,
                    //                       EmployeeId = t.EmployeeId.ToString()
                    //                   }).ToList();

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
                                                OverAllPerformance = 0,
                                                HasKpiGoal = e.HasKpiGoal,
                                                KpiGoalId = e.KpiGoalId,
                                                StartDate = e.ShouldStat.ToString(),
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
                                                    Percentage = y.Target != 0 ? (activityProgress.Where(x => x.QuarterId == y.Id && x.IsApprovedByDirector == approvalStatus.approved && x.IsApprovedByFinance == approvalStatus.approved && x.IsApprovedByManager == approvalStatus.approved).Sum(x => x.ActualWorked) / y.Target) * 100 : 0

                                                }).ToList(),
                                                OverAllProgress = e.Goal != 0 ? activityProgress.Where(x => x.ActivityId == e.Id && x.IsApprovedByDirector == approvalStatus.approved && x.IsApprovedByFinance == approvalStatus.approved && x.IsApprovedByManager == approvalStatus.approved).Sum(x => x.ActualWorked) * 100 / e.Goal : 0,
                                                UsedBudget = activityProgress.Where(x => x.ActivityId == e.Id && x.IsApprovedByFinance == approvalStatus.approved).Sum(x => x.ActualBudget)

                                            }
                                            ).ToList();

                    return new TaskVIewDto
                    {

                        Id = plan.Id,
                        TaskName = plan.PlanName,
                        TaskMembers = taskMembers,
                        TaskMemos = taskMemos,
                        PlannedBudget = plan.PlandBudget,
                        RemainingBudget = plan.PlandBudget - activityViewDtos.Sum(x => x.PlannedBudget),
                        ActivityViewDtos = activityViewDtos,
                        TaskWeight = activityViewDtos.Sum(x => x.Weight),
                        RemianingWeight = 100 - activityViewDtos.Sum(x => x.Weight),
                        NumberofActivities = activityViewDtos.Count(),

                    };
                }

            }
            return new TaskVIewDto();

        }

        public async Task<List<ActivityViewDto>> GetSingleActivityParent(Guid actParentId)
        {
            var activityViewDtos = new List<ActivityViewDto>();
            var activityProgress = _dBContext.ActivityProgresses.Where(x => x.Activity.ActivityParentId == actParentId);
            activityViewDtos.AddRange((from e in _dBContext.Activities.Include(x => x.UnitOfMeasurement)
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
                                           StartDate = e.ShouldStat.ToString(),
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
                                               Percentage = y.Target != 0 ? (activityProgress.Where(x => x.QuarterId == y.Id && x.IsApprovedByDirector == approvalStatus.approved && x.IsApprovedByFinance == approvalStatus.approved && x.IsApprovedByManager == approvalStatus.approved).Sum(x => x.ActualWorked) / y.Target) * 100 : 0
                                           }).ToList(),
                                           OverAllProgress = e.Goal != 0 ? activityProgress.Where(x => x.ActivityId == e.Id && x.IsApprovedByDirector == approvalStatus.approved && x.IsApprovedByFinance == approvalStatus.approved && x.IsApprovedByManager == approvalStatus.approved).Sum(x => x.ActualWorked) * 100 / e.Goal : 0,


                                       }
                                            ).ToList());

            return activityViewDtos;



        }
        public async Task<int> AddTaskMemebers(TaskMembersDto taskMembers)
        {
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

            return 1;
        }
        public async Task<List<SelectListDto>> GetEmployeesNoTaskMembersSelectList(Guid taskId, Guid subOrgId)
        {
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

            return EmployeeSelectList;
        }

        public async Task<List<SelectListDto>> GetTasksSelectList(Guid PlanId)
        {

            return await _dBContext.Tasks.Where(x => x.PlanId == PlanId).
                Select(x => new SelectListDto { Id = x.Id, Name = x.TaskDescription }).ToListAsync();
        }


        public async Task<List<SelectListDto>> GetActivitieParentsSelectList(Guid TaskId)
        {
            return await _dBContext.ActivityParents.Where(x => x.TaskId == TaskId).Select(x => new SelectListDto
            {
                Id = x.Id,
                Name = x.ActivityParentDescription
            }).ToListAsync();
        }

        public async Task<List<SelectListDto>> GetActivitiesSelectList(Guid? planId, Guid? taskId, Guid? actParentId)
        {

            if (planId != null)
            {
                return await _dBContext.Activities.Where(x => x.PlanId == planId)
             .Select(x => new SelectListDto
             {
                 Id = x.Id,
                 Name = x.ActivityDescription
             }).ToListAsync();

            }
            if (taskId != null)
            {
                return await _dBContext.Activities.Where(x => x.TaskId == taskId)
             .Select(x => new SelectListDto
             {
                 Id = x.Id,
                 Name = x.ActivityDescription
             }).ToListAsync();

            }
            return await _dBContext.Activities.Where(x => x.ActivityParentId == actParentId)
                .Select(x => new SelectListDto
                {
                    Id = x.Id,
                    Name = x.ActivityDescription
                }).ToListAsync();
        }


        public async Task<ResponseMessage> UpdateTask(TaskDto updateTask)
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

                    return new ResponseMessage
                    {
                        Success = true,
                        Message = "Task Updated Successfully"
                    };
                }
                return new ResponseMessage
                {
                    Success = false,
                    Message = "Task Not Found"
                };
            }
            catch (Exception ex)
            {
                return new ResponseMessage
                {
                    Success = false,
                    Message = ex.Message.ToString()
                };
            }
        }

        public async Task<ResponseMessage> DeleteTask(Guid taskId)
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

                return new ResponseMessage
                {

                    Success = true,
                    Message = "Task Deleted Successfully !!!"

                };

            }
            return new ResponseMessage
            {

                Success = false,
                Message = "Task Not Found !!!"

            };
        }



    }
}
