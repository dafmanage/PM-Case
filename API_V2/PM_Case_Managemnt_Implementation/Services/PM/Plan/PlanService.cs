using Microsoft.EntityFrameworkCore;
using PM_Case_Managemnt_Implementation.DTOS.Common;
using PM_Case_Managemnt_Implementation.DTOS.PM;
using PM_Case_Managemnt_Implementation.Helpers;
using PM_Case_Managemnt_Infrustructure.Data;
using PM_Case_Managemnt_Infrustructure.Models.Common;
using PM_Case_Managemnt_Infrustructure.Models.PM;

namespace PM_Case_Managemnt_Implementation.Services.PM.Plan
{
    public class PlanService : IPlanService
    {

        private readonly ApplicationDbContext _dBContext;
        public PlanService(ApplicationDbContext context)
        {
            _dBContext = context;
        }

        public async Task<int> CreatePlan(PlanDto plan)
        {
            var budgetYear = await _dBContext.BudgetYears.FindAsync(plan.BudgetYearId);

            var Plans = new PM_Case_Managemnt_Infrustructure.Models.PM.Plan
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

            };





            if (plan.FinanceId != Guid.Empty)
            {
                Plans.FinanceId = plan.FinanceId;
            }
            await _dBContext.AddAsync(Plans);
            await _dBContext.SaveChangesAsync();
            return 1;

        }

        public async Task<List<PlanViewDto>> GetPlans(Guid? programId, Guid SubOrgId)
        {

            var plans = programId != null ? _dBContext.Plans.Include(x => x.Structure).Include(x => x.ProjectManager).Include(x => x.Finance).Where(x => x.ProgramId == programId) :
                _dBContext.Plans.Include(x => x.Structure).Include(x => x.ProjectManager).Include(x => x.Finance).Where(z => z.Structure.SubsidiaryOrganizationId == SubOrgId);


            return await (from p in plans

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




        }


        public async Task<PlanSingleViewDto> GetSinglePlan(Guid planId)
        {

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

            var tasks = (from t in _dBContext.Tasks.Include(z => z.Plan).Where(x => x.PlanId == planId)
                         select new TaskVIewDto
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
                             //NumberOfMembers = _dBContext.TaskMembers.Count(x=>x.TaskId == t.Id),

                             RemianingWeight = (float)(plan.PlanWeight - _dBContext.Activities.Where(x => x.Task.PlanId == planId || x.ActivityParent.Task.PlanId == planId).Sum(x => x.Weight)),
                             NumberofActivities = _dBContext.Activities.Include(x => x.ActivityParent).Count(x => x.TaskId == t.Id || x.ActivityParent.TaskId == t.Id),
                             NumberOfFinalized = _dBContext.Activities.Include(x => x.ActivityParent).Count(x => x.Status == Status.Finalized && (x.TaskId == t.Id || x.ActivityParent.TaskId == t.Id)),
                             NumberOfTerminated = _dBContext.Activities.Include(x => x.ActivityParent).Count(x => x.Status == Status.Terminated && (x.TaskId == t.Id || x.ActivityParent.TaskId == t.Id)),
                             //TaskMembers = (from tm in _dBContext.TaskMembers.Include(x => x.Employee).Where(x => x.TaskId == t.Id)
                             //               select new SelectListDto
                             //               {
                             //                   Id = tm.Id,
                             //                   Name = tm.Employee.FullName,
                             //                   Photo = tm.Employee.Photo,
                             //                   EmployeeId = tm.EmployeeId.ToString()
                             //               }).ToList(),
                             TaskMembers = (from tm in _dBContext.Employees.Where(x => x.OrganizationalStructureId == t.Plan.StructureId)
                                            select new SelectListDto
                                            {
                                                Id = tm.Id,
                                                Name = tm.FullName,
                                                Photo = tm.Photo,
                                                EmployeeId = tm.Id.ToString()
                                            }).ToList(),
                             RemainingBudget = t.PlanedBudget - _dBContext.Activities.Where(x => x.ActivityParent.TaskId == t.Id)
                       .Sum(x => x.PlanedBudget),



                         }).ToList();

            float taskBudgetsum = tasks.Sum(x => x.PlannedBudget);
            float taskweightSum = tasks.Sum(x => x.TaskWeight ?? 0);

            plan.RemainingBudget = plan.RemainingBudget - taskBudgetsum;
            plan.RemainingWeight = float.Parse((plan.PlanWeight - taskweightSum).ToString());
            plan.Tasks = tasks;



            return plan;
        }


        public async Task<List<SelectListDto>> GetPlansSelectList(Guid ProgramId)
        {


            return await _dBContext.Plans.Where(x => x.ProgramId == ProgramId).Select(x => new SelectListDto
            {
                Name = x.PlanName,
                Id = x.Id
            }).ToListAsync();


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

                if (tasks.Any())
                {
                    foreach (var task in tasks)
                    {
                        var taskMemos = await _dBContext.TaskMemos.Where(x => x.TaskId == task.Id).ToListAsync();
                        var taskMembers = await _dBContext.TaskMembers.Where(x => x.TaskId == task.Id).ToListAsync();

                        if (taskMemos.Any())
                        {
                            _dBContext.TaskMemos.RemoveRange(taskMemos);
                            await _dBContext.SaveChangesAsync();
                        }
                        if (taskMembers.Any())
                        {
                            _dBContext.TaskMembers.RemoveRange(taskMembers);
                            await _dBContext.SaveChangesAsync();
                        }

                        var activityParents = await _dBContext.ActivityParents.Where(x => x.TaskId == task.Id).ToListAsync();

                        if (activityParents.Any())
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
                                        if (progAttachments.Any())
                                        {
                                            _dBContext.ProgressAttachments.RemoveRange(progAttachments);
                                            await _dBContext.SaveChangesAsync();
                                        }

                                    }

                                    if (actProgress.Any())
                                    {
                                        _dBContext.ActivityProgresses.RemoveRange(actProgress);
                                        await _dBContext.SaveChangesAsync();
                                    }

                                    var activityTargets = await _dBContext.ActivityTargetDivisions.Where(x => x.ActivityId == act.Id).ToListAsync();


                                    if (activityTargets.Any())
                                    {
                                        _dBContext.ActivityTargetDivisions.RemoveRange(activityTargets);
                                        await _dBContext.SaveChangesAsync();
                                    }


                                    var employees = await _dBContext.EmployeesAssignedForActivities.Where(x => x.ActivityId == act.Id).ToListAsync();


                                    if (activityTargets.Any())
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

                        if (actvities2.Any())
                        {
                            foreach (var act in actvities2)
                            {
                                var actProgress = await _dBContext.ActivityProgresses.Where(x => x.ActivityId == act.Id).ToListAsync();

                                foreach (var actpro in actProgress)
                                {
                                    var progAttachments = await _dBContext.ProgressAttachments.Where(x => x.ActivityProgressId == actpro.Id).ToListAsync();
                                    if (progAttachments.Any())
                                    {
                                        _dBContext.ProgressAttachments.RemoveRange(progAttachments);
                                        await _dBContext.SaveChangesAsync();
                                    }

                                }

                                if (actProgress.Any())
                                {
                                    _dBContext.ActivityProgresses.RemoveRange(actProgress);
                                    await _dBContext.SaveChangesAsync();
                                }

                                var activityTargets = await _dBContext.ActivityTargetDivisions.Where(x => x.ActivityId == act.Id).ToListAsync();


                                if (activityTargets.Any())
                                {
                                    _dBContext.ActivityTargetDivisions.RemoveRange(activityTargets);
                                    await _dBContext.SaveChangesAsync();
                                }


                                var employees = await _dBContext.EmployeesAssignedForActivities.Where(x => x.ActivityId == act.Id).ToListAsync();


                                if (employees.Any())
                                {
                                    _dBContext.EmployeesAssignedForActivities.RemoveRange(employees);
                                    await _dBContext.SaveChangesAsync();
                                }

                                if (activityParents.Any())
                                {
                                    _dBContext.ActivityParents.RemoveRange(activityParents);
                                    await _dBContext.SaveChangesAsync();
                                }



                            }

                            _dBContext.Activities.RemoveRange(actvities2);
                            await _dBContext.SaveChangesAsync();
                        }

                    }
                    _dBContext.Tasks.RemoveRange(tasks);
                    await _dBContext.SaveChangesAsync();


                    _dBContext.Plans.Remove(plan);
                    await _dBContext.SaveChangesAsync();
                }

                else
                {
                    _dBContext.Plans.Remove(plan);
                    await _dBContext.SaveChangesAsync();
                }

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


    }
}
