using System.Net;
using Microsoft.EntityFrameworkCore;
using PM_Case_Managemnt_Implementation.DTOS.Common;
using PM_Case_Managemnt_Implementation.DTOS.PM;
using PM_Case_Managemnt_Implementation.Helpers;
using PM_Case_Managemnt_Implementation.Helpers.Logger;
using PM_Case_Managemnt_Implementation.Helpers.Response;
using PM_Case_Managemnt_Infrustructure.Data;
using PM_Case_Managemnt_Infrustructure.Models.Common;
using PM_Case_Managemnt_Infrustructure.Models.PM;

//
using Tasks = System.Threading.Tasks.Task;

namespace PM_Case_Managemnt_Implementation.Services.PM.Plann
{
    public class PlanService(ApplicationDbContext context) : IPlanService
    {

        private readonly ApplicationDbContext _dBContext;
        private readonly ILoggerManagerService _logger;
        public PlanService(ApplicationDbContext context, ILoggerManagerService logger)
        {
            _dBContext = context;
            _logger = logger;
        }

        public async Task<ResponseMessage<int>> CreatePlan(PlanDto plan)
        {
            var response = new ResponseMessage<int>();
            var budgetYear = await _dBContext.BudgetYears.FindAsync(plan.BudgetYearId);

            var newPlan = new PM_Case_Managemnt_Infrustructure.Models.PM.Plan
            {
                Id = Guid.NewGuid(),
                BudgetYearId = plan.BudgetYearId,
                HasTask = plan.HasTask,
                PlanName = plan.PlanName,
                PlanWeight = plan.PlanWeight,
                PlandBudget = plan.PlandBudget,
                ProgramId = plan.ProgramId,
                ProjectType = plan.ProjectType == 0 ? ProjectType.Capital : ProjectType.Regular,
                Remark = plan.Remark,
                StructureId = plan.StructureId,
                ProjectManagerId = plan.ProjectManagerId,
                ProjectFunder = plan.ProjectFunder,
                PeriodStartAt = budgetYear.FromDate,
                PeriodEndAt = budgetYear.ToDate,
                CreatedAt = DateTime.Now,
                FinanceId = plan.FinanceId != Guid.Empty ? plan.FinanceId : (Guid?)null
            };

            await _dBContext.AddAsync(newPlan);
            await _dBContext.SaveChangesAsync();
            response.Message = "Operation Successful.";
            response.Success = true;
            response.Data = 1;
            _logger.LogCreate("PlanService", plan.Id.ToString(), "Plan Created Successfully");
            return response;

        }

        public async Task<ResponseMessage<List<PlanViewDto>>> GetPlans(Guid? programId, Guid SubOrgId)
        {
            var response = new ResponseMessage<List<PlanViewDto>>();

            var plans = programId != null ? _dBContext.Plans.Include(x => x.Structure).Include(x => x.ProjectManager).Include(x => x.Finance).Where(x => x.ProgramId == programId) :
                _dBContext.Plans.Include(x => x.Structure).Include(x => x.ProjectManager).Include(x => x.Finance).Where(z => z.Structure.SubsidiaryOrganizationId == SubOrgId);


            List<PlanViewDto> result =  await (from p in plans

                          select new PlanViewDto
                          {
                              Id = p.Id,
                              PlanName = p.PlanName,
                              PlanWeight = p.PlanWeight,
                              PlandBudget = p.PlandBudget,
                              StructureName = p.Structure.StructureName,
                              RemainingBudget = p.PlandBudget - _dBContext.Tasks.Where(x => x.PlanId == p.Id).Sum(x => x.PlanedBudget),
                              ProjectManager = p.ProjectManager.FullName,
                              FinanceManager = p.Finance.FullName,
                              Director = _dBContext.Employees.Where(x => x.Position == Position.Director && x.OrganizationalStructureId == p.StructureId).FirstOrDefault().FullName,
                              ProjectType = p.ProjectType.ToString(),
                              NumberOfTask = _dBContext.Tasks.Count(x => x.PlanId == p.Id),
                              NumberOfActivities = _dBContext.Activities.Include(x => x.ActivityParent.Task.Plan).Where(x => x.PlanId == p.Id || x.Task.PlanId == p.Id || x.ActivityParent.Task.PlanId == p.Id).Count(),
                              NumberOfTaskCompleted = _dBContext.Activities.Include(x => x.ActivityParent.Task.Plan).Where(x => x.Status == Status.Finalized && (x.PlanId == p.Id || x.Task.PlanId == p.Id || x.ActivityParent.Task.PlanId == p.Id)).Count(),
                              HasTask = p.HasTask,
                              BudgetYearId = p.BudgetYearId,
                              ProgramId = p.ProgramId,
                              Remark = p.Remark,
                              StructureId = p.StructureId,
                              ProjectManagerId = p.ProjectManagerId,
                              FinanceId = p.FinanceId,
                              ProjectFunder = p.ProjectFunder,
                              BranchId = p.Structure.OrganizationBranchId
                          }).ToListAsync();

            response.Message = "Operation Successful.";
            response.Success = true;
            response.Data = result;

            return response;


        }


        public async Task<ResponseMessage<PlanSingleViewDto>> GetSinglePlan(Guid planId)
        {

            var response = new ResponseMessage<PlanSingleViewDto>();

            var plan = await (from p in _dBContext.Plans.Where(x => x.Id == planId)
                              select new PlanSingleViewDto
                              {
                                  Id = p.Id,
                                  PlanName = p.PlanName,
                                  PlanWeight = p.PlanWeight,
                                  PlannedBudget = p.PlandBudget,
                                  RemainingBudget = p.PlandBudget,
                                  //RemainingWeight = float.Parse((100.0 - taskweightSum).ToString()),
                                  EndDate = p.PeriodEndAt.ToString(),
                                  StartDate = p.PeriodStartAt.ToString(),
                                  //Tasks = tasks
                              }).FirstOrDefaultAsync();

                              }).FirstOrDefaultAsync();

        if (plan == null)
            {
                response.Message = "Plan not found.";
                response.Success = false;
                response.Data = null;
                response.ErrorCode = HttpStatusCode.NotFound.ToString();

                return response;
            }

    var tasks = await _dBContext.Tasks
                .Include(t => t.Plan)
                .Where(t => t.PlanId == planId)
                .Select(t => new TaskViewDto
                {
                    Id = t.Id,
                    TaskName = t.TaskDescription,
                    TaskWeight = t.Weight,
                    FinishedActivitiesNo = 0,
                    TerminatedActivitiesNo = 0,
                    StartDate = t.ShouldStartPeriod ?? DateTime.Now,
                    EndDate = t.ShouldEnd ?? DateTime.Now,
                    HasActivity = t.HasActivityParent,
                    PlannedBudget = t.PlanedBudget,
                    RemainingWeight = (float)(plan.PlanWeight - _dBContext.Activities
                        .Where(a => a.Task.PlanId == planId || a.ActivityParent.Task.PlanId == planId)
                        .Sum(a => a.Weight)),
                    NumberofActivities = _dBContext.Activities
                        .Include(a => a.ActivityParent)
                        .Count(a => a.TaskId == t.Id || a.ActivityParent.TaskId == t.Id),
                    NumberOfFinalized = _dBContext.Activities
                        .Include(a => a.ActivityParent)
                        .Count(a => a.Status == Status.Finalized && (a.TaskId == t.Id || a.ActivityParent.TaskId == t.Id)),
                    NumberOfTerminated = _dBContext.Activities
                        .Include(a => a.ActivityParent)
                        .Count(a => a.Status == Status.Terminated && (a.TaskId == t.Id || a.ActivityParent.TaskId == t.Id)),
                    TaskMembers = _dBContext.Employees
                        .Where(e => e.OrganizationalStructureId == t.Plan.StructureId)
                        .Select(e => new SelectListDto
                        {
                            Id = e.Id,
                            Name = e.FullName,
                            Photo = e.Photo,
                            EmployeeId = e.Id.ToString()
                        })
                        .ToList(),
                    RemainingBudget = t.PlanedBudget - _dBContext.Activities
                        .Where(a => a.ActivityParent.TaskId == t.Id)
                        .Sum(a => a.PlanedBudget)
                })
                .ToListAsync();

            float taskBudgetSum = tasks.Sum(x => x.PlannedBudget);
            float taskWeightSum = tasks.Sum(x => x.TaskWeight ?? 0);

            plan.RemainingBudget -= taskBudgetSum;
            plan.RemainingWeight = float.Parse((plan.PlanWeight - taskWeightSum).ToString());
            plan.Tasks = tasks;



            response.Message = "Operation Successful.";
            response.Success = true;
            response.Data = plan;

            return response;
        }


        public async Task<ResponseMessage<List<SelectListDto>>> GetPlansSelectList(Guid ProgramId)
        {

            var response = new ResponseMessage<List<SelectListDto>>();


            List<SelectListDto> result =  await _dBContext.Plans.Where(x => x.ProgramId == ProgramId).Select(x => new SelectListDto
            {
                Name = x.PlanName,
                Id = x.Id
            }).ToListAsync();
            
            response.Message = "Operation Successful.";
            response.Success = true;
            response.Data = result;

            return response;

        }

        public async Task<ResponseMessage> UpdatePlan(PlanDto plan)
        {
            try
            {
                var singlePlan = await _dBContext.Plans.FindAsync(plan.Id);
                var budgetYear = await _dBContext.BudgetYears.FindAsync(plan.BudgetYearId);

                if (singlePlan != null)
                {
                    singlePlan.HasTask = plan.HasTask;
                    singlePlan.BudgetYearId = plan.BudgetYearId;
                    singlePlan.PlanName = plan.PlanName;
                    singlePlan.PlanWeight = plan.PlanWeight;
                    singlePlan.PlandBudget = plan.PlandBudget;
                    singlePlan.ProgramId = plan.ProgramId;
                    singlePlan.ProjectType = plan.ProjectType == 0 ? ProjectType.Capital : ProjectType.Regular;
                    singlePlan.Remark = plan.Remark;
                    singlePlan.StructureId = plan.StructureId;
                    singlePlan.ProjectManagerId = plan.ProjectManagerId;
                    singlePlan.ProjectFunder = plan.ProjectFunder;
                    singlePlan.PeriodStartAt = budgetYear.FromDate;
                    singlePlan.PeriodEndAt = budgetYear.ToDate;

                    if (plan.ProjectManagerId != Guid.Empty)
                    {
                        singlePlan.ProjectManagerId = plan.ProjectManagerId;

                    }
                    await _dBContext.SaveChangesAsync();

                }
                _logger.LogUpdate("PlanService", plan.Id.ToString(), "Plan Updated Successfully");

                return new ResponseMessage
                {
                    Success = true,
                    Message = "Project Updated Successfully"
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
        public async Task<ResponseMessage> DeletePlan(Guid planId)
        {
            var plan = await _dBContext.Plans.FindAsync(planId);

            if (plan == null)
            {
                return new ResponseMessage
                {
                    Message = "Project Not Found!!!",
                    Success = false
                };
            }

            try
            {
                var tasks = await _dBContext.Tasks.Where(x => x.PlanId == planId).ToListAsync();

                foreach (var task in tasks)
                {
                    await RemoveTaskRelatedEntities(task.Id);
                    _dBContext.Tasks.Remove(task);
                }

                _dBContext.Plans.Remove(plan);
                await _dBContext.SaveChangesAsync();

                return new ResponseMessage
                {
                    Success = true,
                    Message = "Project Deleted Successfully !!!"
                };
            }
            catch (Exception ex)
            {
                return new ResponseMessage
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        private async Tasks RemoveTaskRelatedEntities(Guid taskId)
        {
            var taskMemos = await _dBContext.TaskMemos.Where(x => x.TaskId == taskId).ToListAsync();
            var taskMembers = await _dBContext.TaskMembers.Where(x => x.TaskId == taskId).ToListAsync();
            var activityParents = await _dBContext.ActivityParents.Where(x => x.TaskId == taskId).ToListAsync();
            var activities = await _dBContext.Activities.Where(x => x.TaskId == taskId).ToListAsync();

            if (taskMemos.Count != 0)
            {
                _dBContext.TaskMemos.RemoveRange(taskMemos);
            }

            if (taskMembers.Count != 0)
            {
                _dBContext.TaskMembers.RemoveRange(taskMembers);
            }

            foreach (var activityParent in activityParents)
            {
                await RemoveActivityRelatedEntities(activityParent.Id);
                _dBContext.ActivityParents.Remove(activityParent);
            }

            foreach (var activity in activities)
            {
                await RemoveActivityRelatedEntities(activity.Id);
                _dBContext.Activities.Remove(activity);
            }

            await _dBContext.SaveChangesAsync();
        }

        private async Tasks RemoveActivityRelatedEntities(Guid activityId)
        {
            var activityProgresses = await _dBContext.ActivityProgresses.Where(x => x.ActivityId == activityId).ToListAsync();
            var activityTargets = await _dBContext.ActivityTargetDivisions.Where(x => x.ActivityId == activityId).ToListAsync();
            var employees = await _dBContext.EmployeesAssignedForActivities.Where(x => x.ActivityId == activityId).ToListAsync();

            foreach (var progress in activityProgresses)
            {
                var progressAttachments = await _dBContext.ProgressAttachments.Where(x => x.ActivityProgressId == progress.Id).ToListAsync();
                if (progressAttachments.Count != 0)
                {
                    _dBContext.ProgressAttachments.RemoveRange(progressAttachments);
                }
            }

            if (activityProgresses.Count != 0)
            {
                _dBContext.ActivityProgresses.RemoveRange(activityProgresses);
            }

            if (activityTargets.Count != 0)
            {
                _dBContext.ActivityTargetDivisions.RemoveRange(activityTargets);
            }

            if (employees.Count != 0)
            {
                _dBContext.EmployeesAssignedForActivities.RemoveRange(employees);
            }

            await _dBContext.SaveChangesAsync();
        }
    }
}
