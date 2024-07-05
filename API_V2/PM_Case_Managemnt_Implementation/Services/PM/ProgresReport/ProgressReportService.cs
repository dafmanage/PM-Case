using Microsoft.EntityFrameworkCore;
using PM_Case_Managemnt_Implementation.DTOS.Common;
using PM_Case_Managemnt_Implementation.DTOS.PM;
using PM_Case_Managemnt_Implementation.Helpers;
using PM_Case_Managemnt_Infrustructure.Data;
using PM_Case_Managemnt_Infrustructure.Models.Common;
using PM_Case_Managemnt_Infrustructure.Models.PM;

//
using Task = PM_Case_Managemnt_Infrustructure.Models.PM.Task;

namespace PM_Case_Managemnt_Implementation.Services.PM.ProgresReport
{   
    public class ProgressReportService(ApplicationDbContext context) : IProgressReportService
    {

        private readonly ApplicationDbContext _dBContext = context;

        public async Task<List<DiagramDto>> GetDirectorLevelPerformance(Guid subOrgId, Guid? branchId)
        {
            // Fetch organizational structures and their parent structures eagerly
            var orgStructures = await _dBContext.OrganizationalStructures
                .Where(x => x.SubsidiaryOrganizationId == subOrgId)
                .Include(x => x.ParentStructure)
                .ToListAsync();

            // Extract IDs for filtering employees
            var orgStructureIds = orgStructures.Select(x => x.Id).ToList();

            // Fetch employees belonging to the organizational structures
            var employees = await _dBContext.Employees
                .Where(x => orgStructureIds.Contains(x.OrganizationalStructureId))
                .ToListAsync();

            // Fetch ProgramBudgetYears IDs for the subsidiary organization
            var programBudgetYearsIds = await _dBContext.ProgramBudgetYears
                .Where(x => x.SubsidiaryOrganizationId == subOrgId)
                .Select(x => x.Id)
                .ToListAsync();

            // Fetch the active BudgetYear for the subsidiary organization
            var BudgetYear = await _dBContext.BudgetYears
                .Where(x => programBudgetYearsIds.Contains(x.ProgramBudgetYearId) && x.RowStatus == RowStatus.Active)
                .SingleOrDefaultAsync();

            // Initialize the root node for the diagram
            var parentStructure = orgStructures.FirstOrDefault(x => x.ParentStructureId == null);
            var rootDiagramDto = CreateDiagramDto(parentStructure, employees, BudgetYear);

            // Build the tree structure
            BuildTree(orgStructures, rootDiagramDto, employees, BudgetYear);

            // Return the root node wrapped in a list
            return [rootDiagramDto];
        }

        private DiagramDto CreateDiagramDto(OrganizationalStructure structure, List<Employee> employees, BudgetYear BudgetYear)
        {
            var director = employees.FirstOrDefault(e => e.OrganizationalStructureId == structure.Id && e.Position == Position.Director);
            var contribution = GetContribution(structure.Id, structure.Weight, BudgetYear.Id);

            return new DiagramDto
            {
                Data = new 
                {
                    name = structure.StructureName,
                    weight = contribution,
                    head = $"{director?.Title} {director?.FullName}"
                },
                Label = structure.StructureName,
                Expanded = true,
                Type = "organization",
                StyleClass = contribution < 25.00 ? "bg-danger text-white" : "bg-success text-white",
                Id = structure.Id,
                Order = structure.Order,
                Children = []
            };
        }

        private float GetContribution(Guid structureId, float weight, Guid budetyearId)
        {
            float performance = 0;
            float contribution = 0;
            float Progress = 0;
            var Plans = _dBContext.Plans
                .Include(x => x.Activities)
                .Include(x => x.Tasks).ThenInclude(a => a.Activities)
                .Include(x => x.Tasks).ThenInclude(a => a.ActivitiesParents).ThenInclude(a => a.Activities)
                .Where(x => x.StructureId == structureId && x.BudgetYearId == budetyearId).ToList();

            float Pro_BeginingPlan = 0;
            float Pro_ActualPlan = 0;
            float Pro_Goal = 0;


            foreach (var planItems in Plans)
            {
                float BeginingPlan = 0;
                float ActualPlan = 0;
                float Goal = 0;
                var Tasks = planItems.Tasks;

                if (planItems.HasTask)
                {
                    foreach (var taskItems in Tasks)
                    {


                        if (taskItems.HasActivityParent)
                        {

                            foreach (var actparent in taskItems.ActivitiesParents)
                            {
                                var Activities = actparent.Activities;
                                float BeginingPercent = 0;
                                float ActualWorkedPercent = 0;
                                float GoalPercent = 0;
                                if (Activities.Count == 0)
                                {
                                    Goal += planItems.PlanWeight;
                                }
                                foreach (var activityItems in Activities)
                                {
                                    BeginingPercent += activityItems.Begining * activityItems.Weight / Activities.Sum(x => x.Weight);
                                    ActualWorkedPercent += activityItems.ActualWorked * activityItems.Weight / Activities.Sum(x => x.Weight);
                                    GoalPercent += activityItems.Goal * activityItems.Weight / Activities.Sum(x => x.Weight);

                                }
                                float taskItemsWeight = actparent.Weight == null ? 0 : (float)actparent.Weight;
                                BeginingPlan += BeginingPercent * taskItemsWeight / (float)taskItems.Weight;
                                ActualPlan += ActualWorkedPercent * taskItemsWeight / (float)taskItems.Weight;
                                Goal += GoalPercent * taskItemsWeight / (float)taskItems.Weight;
                            }



                        }
                        else
                        {
                            var Activities = taskItems.Activities;
                            float BeginingPercent = 0;
                            float ActualWorkedPercent = 0;
                            float GoalPercent = 0;
                            if (Activities.Count == 0)
                            {
                                Goal += planItems.PlanWeight;
                            }
                            foreach (var activityItems in Activities)
                            {
                                BeginingPercent += activityItems.Begining * activityItems.Weight / Activities.Sum(x => x.Weight);
                                ActualWorkedPercent += activityItems.ActualWorked * activityItems.Weight / Activities.Sum(x => x.Weight);
                                GoalPercent += activityItems.Goal * activityItems.Weight / Activities.Sum(x => x.Weight);

                            }
                            float taskItemsWeight = taskItems.Weight == null ? 0 : (float)taskItems.Weight;
                            BeginingPlan += BeginingPercent * taskItemsWeight / planItems.PlanWeight;
                            ActualPlan += ActualWorkedPercent * taskItemsWeight / planItems.PlanWeight;
                            Goal += GoalPercent * taskItemsWeight / planItems.PlanWeight;
                        }
                    }
                }

                else
                {
                    var Activities = planItems.Activities;
                    float BeginingPercent = 0;
                    float ActualWorkedPercent = 0;
                    float GoalPercent = 0;
                    if (Activities.Count == 0)
                    {
                        Goal += planItems.PlanWeight;
                    }
                    foreach (var activityItems in Activities)
                    {
                        BeginingPercent += activityItems.Begining * activityItems.Weight / Activities.Sum(x => x.Weight);
                        ActualWorkedPercent += activityItems.ActualWorked * activityItems.Weight / Activities.Sum(x => x.Weight);
                        GoalPercent += activityItems.Goal * activityItems.Weight / Activities.Sum(x => x.Weight);

                    }
                    
                    BeginingPlan += BeginingPercent;
                    ActualPlan += ActualWorkedPercent;
                    Goal += GoalPercent;

                }
                Pro_BeginingPlan += BeginingPlan * (float)planItems.PlanWeight / 100;
                Pro_ActualPlan += ActualPlan * (float)planItems.PlanWeight / 100;
                Pro_Goal += Goal * (float)planItems.PlanWeight / 100;
            }
            if (Pro_ActualPlan > 0)
            {
                if (Pro_ActualPlan == Pro_Goal)
                {
                    Progress = 100;
                }
                else
                {
                    float Nominator = Pro_ActualPlan;
                    float Denominator = Pro_Goal;
                    Progress = Nominator / Denominator * 100;
                }
            }
            else Progress = 0;
            
            performance = (float)Progress;
            contribution = performance * weight / 100;
            performance = (float)Math.Round(performance, 2);
            contribution = (float)Math.Round(contribution, 2);

            return contribution;
        }


        private void BuildTree(List<OrganizationalStructure> orgStructures, DiagramDto parentNode, List<Employee> employees, BudgetYear BudgetYear)
        {
            var childStructures = orgStructures.Where(x => x.ParentStructureId == parentNode.Id).OrderBy(x => x.Order);

            foreach (var childStructure in childStructures)
            {
                var childNode = CreateDiagramDto(childStructure, employees, BudgetYear);
                parentNode.Children.Add(childNode);
                BuildTree(orgStructures, childNode, employees, BudgetYear);
            }
        }

        public async Task<PlanReportByProgramDto> PlanReportByProgram(Guid subOrgId, string BudgetYear, string reportBy)
        {
            var planReportByProgramDto = new PlanReportByProgramDto();

            if (BudgetYear == null) return planReportByProgramDto;

            try
            {
                int BudgetYearPlan = Convert.ToInt32(BudgetYear);
                var BudgetYearValue = _dBContext.BudgetYears.Single(x => x.Year == BudgetYearPlan);
                var programLists = _dBContext.Programs
                    .Where(x => x.ProgramBudgetYearId == BudgetYearValue.ProgramBudgetYearId && x.SubsidiaryOrganizationId == subOrgId)
                    .OrderBy(x => x.CreatedAt)
                    .ToList();

                var programViewModelList = programLists.Select(program => new ProgramViewModel
                {
                    ProgramName = program.ProgramName,
                    ProgramPlanViewModels = GetProgramPlanViewModels(program.Id)
                }).ToList();

                planReportByProgramDto.ProgramViewModels = programViewModelList;
                planReportByProgramDto.MonthCounts = CalculateMonthCounts(programViewModelList, reportBy);
            }
            catch (Exception)
            {
                //TODO: Log exception here
            }

            return planReportByProgramDto;
        }

        private List<ProgramPlanViewModel> GetProgramPlanViewModels(Guid programId)
        {
            var plansList = _dBContext.Plans
                .Where(x => x.ProgramId == programId)
                .OrderBy(x => x.CreatedAt)
                .ToList();

            return plansList.Select(plan => plan.HasTask ? GetTaskBasedProgramPlanViewModel(plan) : GetActivityBasedProgramPlanViewModel(plan)).ToList();
        }

        private ProgramPlanViewModel GetActivityBasedProgramPlanViewModel(Plan plan)
        {
            var ProgramPlanData = new ProgramPlanViewModel
            {
                FiscalPlanPrograms = [],
                ProgramName = plan.Program.ProgramName
            };

            var PlanActivities = _dBContext.Activities.Include(x => x.UnitOfMeasurement).SingleOrDefault(x => x.PlanId == plan.Id);
            if (PlanActivities != null)
            {
                ProgramPlanData.MeasurementUnit = PlanActivities.UnitOfMeasurement.Name;
                ProgramPlanData.PlanName = plan.PlanName;
                ProgramPlanData.TotalBirr = plan.PlandBudget;
                ProgramPlanData.TotalGoal = plan.Activities.Sum(x => x.Goal);

                var TargetActivities = _dBContext.ActivityTargetDivisions.Where(x => x.ActivityId == PlanActivities.Id).OrderBy(x => x.Order).ToList();
                foreach (var tarItems in TargetActivities)
                {
                    var progs = new FiscalPlanProgram
                    {
                        RowOrder = tarItems.Order,
                        FinancialValue = tarItems.TargetBudget,
                        FiscalValue = tarItems.Target
                    };
                    ProgramPlanData.FiscalPlanPrograms.Add(progs);
                }
            }

            return ProgramPlanData;
        }

        private ProgramPlanViewModel GetTaskBasedProgramPlanViewModel(Plan plan)
        { 
            var ProgramPlanData = new ProgramPlanViewModel
            {
                FiscalPlanPrograms = [],
                ProgramName = plan.Program.ProgramName
            };

            var TasksOnPlan = _dBContext.Tasks.Where(x => x.PlanId == plan.Id).OrderBy(x => x.CreatedAt).ToList();
            float TotalBirr = 0;

            List<FiscalPlanProgram> fsForPlan = [];

            foreach (var taskitems in TasksOnPlan)
            {
                if (!taskitems.HasActivityParent)
                {
                    var TaskActivities = _dBContext.Activities.Include(x => x.UnitOfMeasurement).FirstOrDefault(x => x.TaskId == taskitems.Id);
                    if (TaskActivities != null)
                    {
                        TotalBirr += taskitems.PlanedBudget;
                        string MeasurementName = TaskActivities.UnitOfMeasurement.Name;
                        var TargetForTasks = _dBContext.ActivityTargetDivisions.Where(x => x.ActivityId == TaskActivities.Id).OrderBy(x => x.Order).ToList();

                        foreach (var tarItems in TargetForTasks)
                        {
                            var existing = fsForPlan.FirstOrDefault(x => x.RowOrder == tarItems.Order);
                            if (existing == null)
                            {
                                var progs = new FiscalPlanProgram
                                {
                                    RowOrder = tarItems.Order,
                                    FinancialValue = tarItems.TargetBudget,
                                    FiscalValue = tarItems.Target * tarItems.Activity.Weight / _dBContext.Activities.Where(x => x.TaskId == tarItems.Activity.TaskId).Sum(x => x.Weight)
                                };
                                progs.FiscalValue = (UInt32)Math.Round(progs.FiscalValue, 2);
                                fsForPlan.Add(progs);
                            }
                            else
                            {
                                existing.FinancialValue += tarItems.TargetBudget;
                                existing.FiscalValue += tarItems.Target * tarItems.Activity.Weight / _dBContext.Activities.Where(x => x.TaskId == tarItems.Activity.TaskId).Sum(x => x.Weight);
                            }
                        }
                    }
                }
                else
                {
                    var ParentActivities = _dBContext.ActivityParents.Where(x => x.TaskId == taskitems.Id).ToList();
                    TotalBirr += ParentActivities.Sum(x => x.PlanedBudget);

                    foreach (var pAct in ParentActivities)
                    {
                        foreach (var SubAct in pAct.Activities)
                        {
                            var TargetForTasks = _dBContext.ActivityTargetDivisions.Where(x => x.ActivityId == SubAct.Id).OrderBy(x => x.Order).ToList();

                            foreach (var tarItems in TargetForTasks)
                            {
                                var existing = fsForPlan.FirstOrDefault(x => x.RowOrder == tarItems.Order);
                                if (existing == null)
                                {
                                    var progs = new FiscalPlanProgram
                                    {
                                        RowOrder = tarItems.Order,
                                        FinancialValue = tarItems.TargetBudget,
                                        FiscalValue = tarItems.Target * tarItems.Activity.Weight / (float)tarItems.Activity.ActivityParent.Task.Weight
                                    };
                                    progs.FiscalValue = (UInt32)Math.Round(progs.FiscalValue, 2);
                                    fsForPlan.Add(progs);
                                }
                                else
                                {
                                    existing.FinancialValue += tarItems.TargetBudget;
                                    existing.FiscalValue += tarItems.Target * tarItems.Activity.Weight / (float)tarItems.Activity.ActivityParent.Task.Weight;
                                    existing.FiscalValue = (UInt32)Math.Round(existing.FiscalValue, 2);
                                }
                            }
                        }
                    }
                }
            }

            ProgramPlanData.PlanName = plan.PlanName;
            ProgramPlanData.TotalBirr = TotalBirr;
            ProgramPlanData.TotalGoal = fsForPlan.Sum(x => x.FiscalValue);
            ProgramPlanData.MeasurementUnit = fsForPlan.First().MeasurementName; 
            ProgramPlanData.FiscalPlanPrograms.AddRange(fsForPlan);

            return ProgramPlanData;
        }

        private static List<FiscalPlanProgram> CalculateMonthCounts(List<ProgramViewModel> programViewModelList, string reportBy)
        {
            var MonthDeclarator = programViewModelList.FirstOrDefault(x => x.ProgramPlanViewModels.Count > 0)?.ProgramPlanViewModels.FirstOrDefault(x => x.FiscalPlanPrograms.Count > 0)?.FiscalPlanPrograms.ToList();

            if (MonthDeclarator == null || MonthDeclarator.Count == 0) return [];

            if (reportBy == "Quarter")
            {
                for (int i = 0, j = 3; i < 4; i++, j += 3)
                {
                    foreach (var progs in programViewModelList)
                    {
                        foreach (var plns in progs.ProgramPlanViewModels)
                        {
                            var newMonth = plns.FiscalPlanPrograms.Where(x => x.RowOrder <= j && x.MonthName == null);
                            var planProgram = new FiscalPlanProgram
                            {
                                RowOrder = i + 1,
                                MonthName = "Quarter " + (i + 1),
                                FiscalValue = newMonth.Sum(x => x.FiscalValue),
                                FinancialValue = newMonth.Sum(x => x.FinancialValue)
                            };
                            plns.FiscalPlanPrograms.Add(planProgram);
                            plns.FiscalPlanPrograms.RemoveAll(x => newMonth.Contains(x));
                        }
                    }
                }
            }
            else
            {
                int newI = 0;
                for (int i = 1; i <= MonthDeclarator.Count(); i++)
                {
                    int h = i >= 7 ? i - 6 : i + 6;
                    var mfi = new System.Globalization.DateTimeFormatInfo();
                    string strMonthName = mfi.GetMonthName(h);
                    MonthDeclarator[newI].MonthName = strMonthName;
                    newI++;
                }
            }

            return [.. MonthDeclarator];
        }

        public async Task<PlanReportDetailDto> StructureReportByProgram(string BudgetYear, string programId, string reportBy)
        {
            PlanReportDetailDto planReportDetailDto = new();
            if (string.IsNullOrEmpty(BudgetYear)) return planReportDetailDto;

            try
            {
                var monthDeclarator = new List<ActivityTargetDivisionReport>();
                var programWithStructure = new List<ProgramWithStructure>();
                Guid newProgram = Guid.Parse(programId);

                var structures = GetStructuresInProgram(newProgram);
                foreach (var structure in structures)
                {
                    var progWithStructureItem = new ProgramWithStructure
                    {
                        StructureName = structure.StructureName,
                        StructurePlans = []
                    };

                    var plansInStructure = GetPlansInStructure(structure.Id, newProgram);
                    foreach (var plan in plansInStructure)
                    {
                        var structurePlan = CreateStructurePlan(plan, monthDeclarator);
                        progWithStructureItem.StructurePlans.Add(structurePlan);
                    }

                    programWithStructure.Add(progWithStructureItem);
                }

                if (monthDeclarator.Count != 0)
                {
                    if (reportBy == "Quarter")
                    {
                        monthDeclarator = CreateQuarterlyReport(programWithStructure);
                    }
                    else
                    {
                        AssignMonthNames(monthDeclarator);
                    }
                }

                planReportDetailDto.MonthCounts = monthDeclarator;
                planReportDetailDto.ProgramWithStructure = programWithStructure;
            }
            catch (Exception)
            {
                //TODO; Handle exception (logging, rethrowing, etc.)
            }

            return planReportDetailDto;
        }

        private List<OrganizationalStructure> GetStructuresInProgram(Guid programId)
        {
            var structureIds = _dBContext.Plans
                .Where(x => x.ProgramId == programId)
                .Select(x => x.StructureId)
                .Distinct()
                .ToList();

            return [.. _dBContext.OrganizationalStructures.Where(x => structureIds.Contains(x.Id))];
        }

        private List<Plan> GetPlansInStructure(Guid structureId, Guid programId)
        {
            return [.. _dBContext.Plans
                .Include(x => x.Tasks).ThenInclude(x => x.ActivitiesParents).ThenInclude(x => x.Activities).ThenInclude(x => x.UnitOfMeasurement)
                .Include(x => x.Tasks).ThenInclude(x => x.Activities).ThenInclude(x => x.UnitOfMeasurement)
                .Include(x => x.Activities).ThenInclude(x => x.UnitOfMeasurement)
                .Include(x => x.Tasks).ThenInclude(x => x.ActivitiesParents).ThenInclude(x => x.Activities).ThenInclude(x => x.ActivityTargetDivisions)
                .Include(x => x.Tasks).ThenInclude(x => x.Activities).ThenInclude(x => x.ActivityTargetDivisions)
                .Include(x => x.Activities).ThenInclude(x => x.ActivityTargetDivisions)
                .Where(x => x.StructureId == structureId && x.ProgramId == programId)
                .OrderBy(x => x.CreatedAt)];
        }

        private StructurePlan CreateStructurePlan(Plan plan, List<ActivityTargetDivisionReport> monthDeclarator)
        {
            var structurePlan = new StructurePlan
            {
                PlanName = plan.PlanName,
                PlanTasks = plan.HasTask ? CreatePlanTasks(plan.Tasks, monthDeclarator) : null,
                PlanTargetDivision = plan.HasTask ? null : CreateActivityTargetDivisionReports(plan.Activities.FirstOrDefault(), monthDeclarator),
                Weight = plan.HasTask ? 0 : plan.Activities.FirstOrDefault()?.Weight ?? 0,
                Target = plan.HasTask ? 0 : plan.Activities.FirstOrDefault()?.Goal ?? 0,
                UnitOfMeasurement = plan.HasTask ? null : plan.Activities.FirstOrDefault()?.UnitOfMeasurement.Name
            };

            return structurePlan;
        }

        private List<PlanTask> CreatePlanTasks(IEnumerable<Task> tasks, List<ActivityTargetDivisionReport> monthDeclarator)
        {
            var planTasks = new List<PlanTask>();
            foreach (var task in tasks.OrderBy(x => x.CreatedAt))
            {
                var planTask = new PlanTask
                {
                    TaskName = task.TaskDescription,
                    TaskActivities = task.HasActivityParent ? CreateTaskActivities(task.ActivitiesParents, monthDeclarator) : null,
                    TaskTargetDivision = task.HasActivityParent ? null : CreateActivityTargetDivisionReports(task.Activities.FirstOrDefault(), monthDeclarator),
                    Weight = task.HasActivityParent ? 0 : task.Activities.FirstOrDefault()?.Weight ?? 0,
                    Target = task.HasActivityParent ? 0 : task.Activities.FirstOrDefault()?.Goal ?? 0,
                    UnitOfMeasurement = task.HasActivityParent ? null : task.Activities.FirstOrDefault()?.UnitOfMeasurement.Name
                };

                planTasks.Add(planTask);
            }

            return planTasks;
        }

        private List<TaskActivity> CreateTaskActivities(IEnumerable<ActivityParent> activityParents, List<ActivityTargetDivisionReport> monthDeclarator)
        {
            var taskActivities = new List<TaskActivity>();
            foreach (var activityParent in activityParents.OrderBy(x => x.CreatedAt))
            {
                var taskActivity = new TaskActivity
                {
                    ActivityName = activityParent.ActivityParentDescription,
                    ActSubActivity = activityParent.HasActivity ? CreateSubActivities(activityParent.Activities, monthDeclarator) : null,
                    ActivityTargetDivision = activityParent.HasActivity ? null : CreateActivityTargetDivisionReports(activityParent.Activities.FirstOrDefault(), monthDeclarator),
                    Weight = activityParent.HasActivity ? 0 : activityParent.Activities.FirstOrDefault()?.Weight ?? 0,
                    Target = activityParent.HasActivity ? 0 : activityParent.Activities.FirstOrDefault()?.Goal ?? 0,
                    UnitOfMeasurement = activityParent.HasActivity ? null : activityParent.Activities.FirstOrDefault()?.UnitOfMeasurement.Name
                };

                taskActivities.Add(taskActivity);
            }

            return taskActivities;
        }

        private List<ActSubActivity> CreateSubActivities(IEnumerable<Activity> activities, List<ActivityTargetDivisionReport> monthDeclarator)
        {
            var subActivities = new List<ActSubActivity>();
            foreach (var activity in activities.OrderBy(x => x.CreatedAt))
            {
                var subActivity = new ActSubActivity
                {
                    SubActivityDescription = activity.ActivityDescription,
                    Weight = activity.Weight,
                    Target = activity.Goal,
                    UnitOfMeasurement = activity.UnitOfMeasurement.Name,
                    subActivityTargetDivision = CreateActivityTargetDivisionReports(activity, monthDeclarator)
                };

                subActivities.Add(subActivity);
            }

            return subActivities;
        }

        private static List<ActivityTargetDivisionReport> CreateActivityTargetDivisionReports(Activity activity, List<ActivityTargetDivisionReport> monthDeclarator)
        {
            var targetDivisionReports = new List<ActivityTargetDivisionReport>();
            if (activity == null) return targetDivisionReports;

            foreach (var targetDivision in activity.ActivityTargetDivisions)
            {
                var report = new ActivityTargetDivisionReport
                {
                    Order = targetDivision.Order,
                    TargetValue = targetDivision.Target
                };
                targetDivisionReports.Add(report);

                if (!monthDeclarator.Any(x => x.Order == targetDivision.Order))
                {
                    monthDeclarator.Add(report);
                }
            }

            return targetDivisionReports;
        }

        private static List<ActivityTargetDivisionReport> CreateQuarterlyReport(List<ProgramWithStructure> programWithStructure)
        {
            var monthDeclarator = new List<ActivityTargetDivisionReport>();
            int quarterStep = 3;

            for (int i = 0; i < 4; i++)
            {
                int quarterOrder = i + 1;
                int upperLimit = quarterOrder * quarterStep;
                var quarterReport = new ActivityTargetDivisionReport
                {
                    Order = quarterOrder, 
                    MonthName = $"Quarter {quarterOrder}"
                };

                foreach (var structure in programWithStructure)
                {
                    foreach (var plan in structure.StructurePlans)
                    {
                        if (plan.PlanTargetDivision != null)
                        {
                            var targetValues = plan.PlanTargetDivision.Where(x => x.Order <= upperLimit && x.MonthName == null).ToList();
                            quarterReport.TargetValue += targetValues.Sum(x => x.TargetValue);
                            plan.PlanTargetDivision.RemoveAll(x => targetValues.Contains(x));
                        }
                        else
                        {
                            foreach (var task in plan.PlanTasks)
                            {
                                if (task.TaskTargetDivision != null)
                                {
                                    var targetValues = task.TaskTargetDivision.Where(x => x.Order <= upperLimit && x.MonthName == null).ToList();
                                    quarterReport.TargetValue += targetValues.Sum(x => x.TargetValue);
                                    task.TaskTargetDivision.RemoveAll(x => targetValues.Contains(x));
                                }
                                else
                                {
                                    foreach (var activity in task.TaskActivities)
                                    {
                                        if (activity.ActivityTargetDivision != null)
                                        {
                                            var targetValues = activity.ActivityTargetDivision.Where(x => x.Order <= upperLimit && x.MonthName == null).ToList();
                                            quarterReport.TargetValue += targetValues.Sum(x => x.TargetValue);
                                            activity.ActivityTargetDivision.RemoveAll(x => targetValues.Contains(x));
                                        }
                                        else
                                        {
                                            foreach (var subActivity in activity.ActSubActivity)
                                            {
                                                if (subActivity.subActivityTargetDivision != null)
                                                {
                                                    var targetValues = subActivity.subActivityTargetDivision.Where(x => x.Order <= upperLimit && x.MonthName == null).ToList();
                                                    quarterReport.TargetValue += targetValues.Sum(x => x.TargetValue);
                                                    subActivity.subActivityTargetDivision.RemoveAll(x => targetValues.Contains(x));
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (!monthDeclarator.Any(x => x.Order == quarterReport.Order))
                {
                    monthDeclarator.Add(quarterReport);
                }
            }

            return monthDeclarator;
        }

        private static void AssignMonthNames(List<ActivityTargetDivisionReport> monthDeclarator)
        {
            for (int i = 0; i < monthDeclarator.Count; i++)
            {
                int monthNumber = (i + 6) % 12 + 1;
                var monthName = new System.Globalization.DateTimeFormatInfo().GetMonthName(monthNumber);
                monthDeclarator[i].MonthName = monthName;
            }
        }

        public async Task<PlannedReport> PlanReports(string BudgetYear, Guid selectStructureId, string reportBy)
        {
            var plannedReport = new PlannedReport();

            try
            {
                var BudgetYearEntity = _dBContext.BudgetYears.Single(x => x.Year.ToString() == BudgetYear);
                var allPlans = GetAllPlans(selectStructureId, BudgetYearEntity.Id);

                var plansLsts = new List<PlansList>();
                var quarterMonth = new List<QuarterMonth>();

                foreach (var plansItems in allPlans)
                {
                    var plns = CreatePlanList(plansItems);

                    if (plansItems.HasTask)
                    {
                        var TaskLists = CreateTaskLists(plansItems, reportBy, ref quarterMonth, plannedReport);
                        plns.TaskLists = TaskLists;
                    }
                    else if (plansItems.Activities.Any() && plansItems.Activities.FirstOrDefault().ActivityTargetDivisions != null)
                    {
                        var PlanAchievementPlan = CreatePlanAchievementList(plansItems, reportBy, ref quarterMonth, plannedReport);
                        plns.PlanDivision = PlanAchievementPlan;
                    }

                    plansLsts.Add(plns);
                }

                plannedReport.PlansLists = plansLsts;
                return plannedReport;
            }
            catch (Exception)
            {
                return plannedReport;
            }
        }

        private List<Plan> GetAllPlans(Guid selectStructureId, Guid BudgetYearId)
        {
            return [.. _dBContext.Plans
                .Include(x => x.Tasks).ThenInclude(x => x.ActivitiesParents).ThenInclude(x => x.Activities).ThenInclude(x => x.UnitOfMeasurement)
                .Include(x => x.Tasks).ThenInclude(x => x.Activities).ThenInclude(x => x.UnitOfMeasurement)
                .Include(x => x.Activities).ThenInclude(x => x.UnitOfMeasurement)
                .Include(x => x.Tasks).ThenInclude(x => x.ActivitiesParents).ThenInclude(x => x.Activities).ThenInclude(x => x.ActivityTargetDivisions)
                .Include(x => x.Tasks).ThenInclude(x => x.Activities).ThenInclude(x => x.ActivityTargetDivisions)
                .Include(x => x.Activities).ThenInclude(x => x.ActivityTargetDivisions)
                .Where(x => x.StructureId == selectStructureId && x.BudgetYearId == BudgetYearId)
                .OrderBy(c => c.CreatedAt)];
        }

        private PlansList CreatePlanList(Plan plan)
        {
            return new PlansList
            {
                PlanName = plan.PlanName,
                Weight = plan.PlanWeight,
                PlRemark = plan.Remark,
                HasTask = plan.HasTask
            };
        }

        private List<TaskList> CreateTaskLists(Plan plansItems, string reportBy, ref List<QuarterMonth> quarterMonth, PlannedReport plannedReport)
        {
            var TaskLists = new List<TaskList>();
            var tasks = plansItems.Tasks.OrderBy(x => x.CreatedAt);

            foreach (var taskItems in tasks)
            {
                var TaskList = new TaskList
                {
                    TaskDescription = taskItems.TaskDescription,
                    TaskWeight = taskItems.Weight,
                    TRemark = taskItems.Remark,
                    HasActParent = taskItems.HasActivityParent
                };

                if (taskItems.HasActivityParent)
                {
                    var actparentlsts = CreateActivityParentLists(taskItems, reportBy, ref quarterMonth, plannedReport);
                    TaskList.ActParentLst = actparentlsts;
                }
                else if (taskItems.Activities.Any() && taskItems.Activities.FirstOrDefault().ActivityTargetDivisions != null)
                {
                    var planAchievements = CreatePlanAchievementList(taskItems, reportBy, ref quarterMonth, plannedReport);
                    TaskList.TaskDivisions = planAchievements;
                }

                TaskLists.Add(TaskList);
            }

            return TaskLists;
        }

        private List<ActParentLst> CreateActivityParentLists(Task taskItems, string reportBy, ref List<QuarterMonth> quarterMonth, PlannedReport plannedReport)
        {
            var actparentlsts = new List<ActParentLst>();

            foreach (var actparent in taskItems.ActivitiesParents)
            {
                var actparentlst = new ActParentLst
                {
                    ActParentDescription = actparent.ActivityParentDescription,
                    ActParentWeight = actparent.Weight,
                    ActpRemark = actparent.Remark
                };

                if (actparent.Activities != null)
                {
                    var ActivityLists = CreateActivityLists(actparent, reportBy, ref quarterMonth, plannedReport);
                    actparentlst.ActivityLists = ActivityLists;
                }
                else if (actparent.Activities.Any() && actparent.Activities.FirstOrDefault().targetDivision != null)
                {
                    var PlanAchievementPlan = CreatePlanAchievementList(actparent, reportBy, ref quarterMonth, plannedReport);
                    actparentlst.ActDivisions = PlanAchievementPlan;
                }

                actparentlsts.Add(actparentlst);
            }

            return actparentlsts;
        }

        private List<ActivityList> CreateActivityLists(ActivityParent actparent, string reportBy, ref List<QuarterMonth> quarterMonth, PlannedReport plannedReport)
        {
            var ActivityLists = new List<ActivityList>();

            foreach (var actItems in actparent.Activities.Where(x => x.ActivityTargetDivisions != null))
            {
                var lst = new ActivityList
                {
                    ActivityDescription = actItems.ActivityDescription,
                    Begining = actItems.Begining,
                    MeasurementUnit = actItems.UnitOfMeasurement.Name,
                    Target = actItems.Goal,
                    Weight = actItems.Weight,
                    Remark = actItems.Remark,
                    Plans = CreatePlanAchievementList(actItems, reportBy, ref quarterMonth, plannedReport)
                };

                ActivityLists.Add(lst);
            }

            return ActivityLists;
        }

        private List<PlanAchievement> CreatePlanAchievementList(dynamic entity, string reportBy, ref List<QuarterMonth> quarterMonth, PlannedReport plannedReport)
        {
            var planAchievements = new List<PlanAchievement>();

            //TODO: Can be simplified
            var byQuarter = ((IEnumerable<dynamic>)entity.ActivityTargetDivisions)
                .OrderBy(x => x.Order)
                .ToList();

            if (quarterMonth.Count != 0)
            {
                InitializeQuarterMonth(reportBy, ref quarterMonth, plannedReport);
            }

            if (reportBy == "Quarter")
            {
                for (int i = 0; i < 12; i += 3)
                {
                    var planO = new PlanAchievement
                    {
                        Target = byQuarter[i].Target + byQuarter[i + 1].Target + byQuarter[i + 2].Target
                    };
                    planAchievements.Add(planO);
                }
            }
            else
            {
                foreach (var itemQ in byQuarter)
                {
                    var planO = new PlanAchievement
                    {
                        Target = itemQ.Target
                    };
                    planAchievements.Add(planO);
                }
            }

            return planAchievements;
        }

        private void InitializeQuarterMonth(string reportBy, ref List<QuarterMonth> quarterMonth, PlannedReport plannedReport)
        {
            int value = reportBy == "Quarter" ? 4 : 12;

            if (reportBy == "Quarter")
            {
                for (int i = 0; i < 4; i++)
                {
                    var quar = _dBContext.QuarterSettings.Single(x => x.QuarterOrder == i);
                    AdjustMonthRange(quar.StartMonth, quar.EndMonth);
                    
                    var mfi = new System.Globalization.DateTimeFormatInfo();
                    var fromG = mfi.GetMonthName(quar.StartMonth);
                    var toG = mfi.GetMonthName(quar.EndMonth);

                    var quarterMonths = new QuarterMonth
                    {
                        MonthName = "Quarter" + (i + 1)
                    };

                    quarterMonth.Add(quarterMonths);
                }

                plannedReport.PMINT = 4;
            }
            else
            {
                for (int i = 1; i <= 12; i++)
                {
                    int k = i >= 7 ? i - 6 : i + 6;

                    var quarterMonths = new QuarterMonth
                    {
                        MonthName = GetMonthName(k)
                    };

                    quarterMonth.Add(quarterMonths);
                }

                plannedReport.PMINT = 12;
            }

            plannedReport.PlanDurationInQuarter = quarterMonth;
        }

        public ProgressReport GenerateProgressReport(FilterationCriteria filterCriteria)
        {
            var progressReport = new ProgressReport
            {
                AllActivities = []
            };

            try
            {

                string ReportType = "Activity Report for";
                List<QuarterMonth> quarterMonth = [];
                var BudgetYear = _dBContext.BudgetYears.SingleOrDefault(x => x.Year == filterCriteria.BudgetYear);

                if (filterCriteria.ActParentId != Guid.Empty) 
                {

                if (BudgetYear == null) return progressReport;

                var activitiesQuery = _dBContext.Activities
                    .Include(x => x.ActivityTargetDivisions)
                    .Include(x => x.AssignedEmploye)
                    .Include(x => x.Task)
                    .Where(x => x.ActivityParentId == filterCriteria.ActParentId && x.ActivityTargetDivisions != null).ToList();

                var allActivities = FilterActivitiesByBudgetYear(activitiesQuery, filterCriteria.BudgetYear, BudgetYear);

                foreach (var items in activitiesQuery)
                {
                    SetPlanDurationAndQuarterMonths(progressReport, filterCriteria, items, quarterMonth);

                    ProgressReportTable progress = CreateProgressReportTable(items);
                    List<PlanAchievement> planAchievements = CalculatePlanAchievement(items);

                    List<PlanAchievement> planAchievementss = filterCriteria.ReportType == "Quarter"
                        ? AggregateQuarterlyAchievements(planAchievements)
                        : [];

                    progress.PlanAchievement = planAchievementss;
                    progress.Employees = GetEmployees(items);

                    CalculateProgress(progress);

                    if (progress.Progress > 0)
                    {
                        progressReport.AllActivities.Add(progress);
                    }
                }
                }

                else if (filterCriteria.TaskId != Guid.Empty)
                {
                    List<Activity> activities = GetActivities(filterCriteria.TaskId);
                    var allActivities = FilterActivitiesByBudgetYear(activities, filterCriteria.BudgetYear, BudgetYear);

                    progressReport.PlanDuration = filterCriteria.ReportType == "Quarter" ? 4 : 12;

                    if (quarterMonth.Count == 0)
                    {
                        quarterMonth = GenerateQuarterMonths(filterCriteria);
                        progressReport.PlanDurationInQuarter = quarterMonth;
                    }

                    foreach(var item in allActivities){
                        ProgressReportTable progress = CreateProgressReportTable(item);

                        List<PlanAchievement> planAchievements = CalculatePlanAchievement(item);

                        List<PlanAchievement> planAchievementss = filterCriteria.ReportType == "Quarter"
                            ? AggregateQuarterlyAchievements(planAchievements)
                            : [];

                        progress.PlanAchievement = planAchievementss;
                        progress.Employees = GetEmployees(item);

                        CalculateProgress(progress);

                        if (progress.Progress > 0)
                        {
                            progressReport.AllActivities.Add(progress);
                        }
                    }
                }

                else if (filterCriteria.PlanId != Guid.Empty)
                {
                   List<Activity> activities = GetActivities(filterCriteria.PlanId);
                    var allActivities = FilterActivitiesByBudgetYear(activities, filterCriteria.BudgetYear, BudgetYear);

                    progressReport.PlanDuration = filterCriteria.ReportType == "Quarter" ? 4 : 12;

                    if (quarterMonth.Count == 0)
                    {
                        quarterMonth = GenerateQuarterMonths(filterCriteria);
                        progressReport.PlanDurationInQuarter = quarterMonth;
                    }

                    foreach(var item in allActivities){
                        ProgressReportTable progress = CreateProgressReportTable(item);

                        List<PlanAchievement> planAchievements = CalculatePlanAchievement(item);

                        List<PlanAchievement> planAchievementss = filterCriteria.ReportType == "Quarter"
                            ? AggregateQuarterlyAchievements(planAchievements)
                            : [];

                        progress.PlanAchievement = planAchievementss;
                        progress.Employees = GetEmployees(item);

                        CalculateProgress(progress);

                        if (progress.Progress > 0)
                        {
                            progressReport.AllActivities.Add(progress);
                        }
                    }
                }

                progressReport.ReportMessage = $"{ReportType} the Year";  //TODO: Add a meaningful message
                _ = progressReport.AllActivities.OrderByDescending(x => x.Progress).ToList();
            }
            
            catch (Exception e)
            {
                //TODO; Handle exception (logging, rethrowing, etc.)
      
            }

            return progressReport;

        }

        private void SetPlanDurationAndQuarterMonths(ProgressReport progressReport, FilterationCriteria filterationCriteria, Activity items, List<QuarterMonth>  QuarterMonth)
        {
            progressReport.PlanDuration = filterationCriteria.ReportType == "Quarter" ? 4 : 12;

            if (QuarterMonth.Count == 0)
            {
                int value = items.targetDivision == TargetDivision.Quarterly ? 4 : 12;
                int period = filterationCriteria.ReportType == "Quarter" ? 4 : 12;

                for (int i = 0; i < period; i++)
                {
                    int startMonth, endMonth;

                    if (filterationCriteria.ReportType == "Quarter")
                    {
                        var quar = _dBContext.QuarterSettings.Single(x => x.QuarterOrder == i);

                        startMonth = AdjustMonth(quar.StartMonth, 4, 8);
                        endMonth = AdjustMonth(quar.EndMonth, 4, 8);

                        string fromG = GetMonthName(startMonth);
                        string toG = GetMonthName(endMonth);

                        QuarterMonth.Add(new QuarterMonth { MonthName = "Quarter" + (i + 1) });
                    }
                    else
                    {
                        int h = AdjustMonth(i + 1, 6, 6);
                        string strMonthName = GetMonthName(h);

                        QuarterMonth.Add(new QuarterMonth { MonthName = strMonthName });
                    }
                }
                progressReport.PlanDurationInQuarter = QuarterMonth;
            }
        }

        private ProgressReportTable CreateProgressReportTable(Activity items)
        {
            ProgressReportTable progress = new()
            {
                ActivityId = items.Id,
                ActivityDescription = items.ActivityDescription,
                StartDate = items.ShouldStart,
                EndDate = items.ShouldEnd,
                ActualStartDate = items.ActualStart ?? items.ActualStart.Value,
                ActualEndDate = items.ActualEnd ?? items.ActualEnd.Value,
                PlanStartDate = EthiopicDateTime.GetEthiopicDate(items.ShouldStart.Day, items.ShouldStart.Month, items.ShouldStart.Year),
                PlanEndDate = EthiopicDateTime.GetEthiopicDate(items.ShouldEnd.Day, items.ShouldEnd.Month, items.ShouldEnd.Year),
                ProgressStartDate = items.ActualStart == null ? "" : EthiopicDateTime.GetEthiopicDate(items.ActualStart.Value.Day, items.ActualStart.Value.Month, items.ActualStart.Value.Year),
                ProgressEndDate = items.ActualEnd == null ? "" : EthiopicDateTime.GetEthiopicDate(items.ActualEnd.Value.Day, items.ActualEnd.Value.Month, items.ActualEnd.Value.Year),
                UsedBudget = items.ActualBudget,
                PlannedBudget = items.PlanedBudget,
                Status = items.Status,
                Begining = items.Begining,
                ActualWorked = items.ActualWorked,
                Weight = items.Weight,
                Goal = items.Goal
            };

            return progress;
        }

        private List<QuarterMonth> GenerateQuarterMonths(FilterationCriteria filterCriteria)
        {
            List<QuarterMonth> quarterMonths = [];
            if (filterCriteria.ReportType == "Quarter")
            {
                for (int i = 0; i < 4; i++)
                {
                    var quar = _dBContext.QuarterSettings.Single(x => x.QuarterOrder == i);
                    AdjustMonthRange(quar.StartMonth, quar.EndMonth);
                    quarterMonths.Add(new QuarterMonth { MonthName = "Quarter" + (i + 1) });
                }
            }
            else
            {
                for (int i = 1; i <= 12; i++)
                {
                    string strMonthName = new System.Globalization.DateTimeFormatInfo().GetMonthName(i);
                    quarterMonths.Add(new QuarterMonth { MonthName = strMonthName });
                }
            }

            return quarterMonths;
        }

        private List<PlanAchievement> CalculatePlanAchievement(Activity items)
        {
            var byQuarter = _dBContext.ActivityTargetDivisions
                .Where(x => x.ActivityId == items.Id)
                .OrderBy(x => x.Order)
                .ToList();

            List<PlanAchievement> planAchievements = [];

            foreach (var itemQ in byQuarter)
            {
                var progresslist = _dBContext.ActivityProgresses
                    .Where(x => x.ActivityId == items.Id && x.QuarterId == itemQ.Id)
                    .ToList();

                PlanAchievement planO = new()
                {
                    Target = itemQ.Target,
                    Actual = progresslist.Sum(x => x.ActualWorked),
                    PercentageAchieved = itemQ.Target == 0 ? 0 : progresslist.Sum(x => x.ActualWorked) / itemQ.Target * 100
                };

                planAchievements.Add(planO);
            }

            return planAchievements;
        }

        private List<PlanAchievement> AggregateQuarterlyAchievements(List<PlanAchievement> planAchievements)
        {
            List<PlanAchievement> planAchievementss = [];

            for (int i = 0; i < 12; i += 3)
            {
                PlanAchievement planOq = new()
                {
                    Target = planAchievements[i].Target + planAchievements[i + 1].Target + planAchievements[i + 2].Target,
                    Actual = planAchievements[i].Actual + planAchievements[i + 1].Actual + planAchievements[i + 2].Actual,
                    PercentageAchieved = (planAchievements[i].Target + planAchievements[i + 1].Target + planAchievements[i + 2].Target) == 0 ? 0 :
                        (planAchievements[i].Actual + planAchievements[i + 1].Actual + planAchievements[i + 2].Actual) /
                        (planAchievements[i].Target + planAchievements[i + 1].Target + planAchievements[i + 2].Target) * 100
                };

                planAchievementss.Add(planOq);
            }

            return planAchievementss;
        }

        private List<Employee> GetEmployees(Activity items)
        {
            var employees = _dBContext.EmployeesAssignedForActivities
                .Where(x => x.ActivityId == items.Id)
                .Select(z => z.EmployeeId)
                .ToList();

            List<Employee> emp = [];

            if (items.CommiteeId != null)
            {
                var commites = _dBContext.Commitees
                    .Single(x => x.Id == items.CommiteeId)
                    .Employees
                    .Select(x => x.EmployeeId)
                    .ToList();

                emp = [.. _dBContext.Employees.Where(x => commites.Contains(x.Id))];
            }
            else
            {
                emp = [.. _dBContext.Employees.Where(x => employees.Contains(x.Id))];
            }

            return emp;
        }

        private void CalculateProgress(ProgressReportTable progress)
        {
            if (progress.ActualWorked > 0)
            {
                if (progress.ActualWorked == progress.Goal)
                {
                    progress.Progress = 100;
                }
                else
                {
                    float Nominator = progress.ActualWorked;
                    float Denominator = (float)progress.Goal;
                    progress.Progress = Nominator / Denominator * 100;
                }
            }
            else
            {
                progress.Progress = 0;
            }
        }

        private List<Activity> FilterActivitiesByBudgetYear(List<Activity> activities, int? BudgetYear, BudgetYear BudgetYearEntity)
        {
            if (BudgetYear != null)
            {
                var dateType = $"Year of {BudgetYear}";
                return [.. activities
                    .Where(x => x.ShouldStart >= BudgetYearEntity.FromDate && x.ShouldEnd <= BudgetYearEntity.ToDate && x.Goal != 0)
                    .OrderBy(c => c.ShouldStart)];
            }
            return [.. activities.OrderBy(c => c.ShouldStart)];
        }
   
        private int AdjustMonth(int month, int threshold, int offset)
        {
            return month > threshold ? month - threshold : month + offset;
        }

        private void AdjustMonthRange(int startMonth, int endMonth)
        {
            _ = startMonth > 4 ? startMonth - 4 : startMonth + 8;
            _ = endMonth > 4 ? endMonth - 4 : endMonth + 8;
        }

        private string GetMonthName(int month)
        {
            System.Globalization.DateTimeFormatInfo mfi = new();
            return mfi.GetMonthName(month);
        }
        private List<Activity> GetActivities(Guid taskId)
        {
            var task = _dBContext.Tasks
                .Include(x => x.ActivitiesParents).ThenInclude(x => x.Activities).ThenInclude(x => x.ActivityTargetDivisions)
                .Include(x => x.Activities).ThenInclude(x => x.ActivityTargetDivisions)
                .FirstOrDefault(x => x.Id == taskId);

            List<Activity> activities = [];

            if (task.HasActivityParent)
            {
                foreach (var activityParent in task.ActivitiesParents)
                {
                    if (activityParent.HasActivity)
                    {
                        activities.AddRange(activityParent.Activities);
                    }
                    else if (activityParent.Activities.Count != 0 && activityParent.Activities.First().targetDivision != null)
                    {
                        activities.Add(activityParent.Activities.First());
                    }
                }
            }
            else if (task.Activities.Count != 0 && task.Activities.First().ActivityTargetDivisions.Count != 0)
            {
                activities.Add(task.Activities.First());
            }

            return activities;
        }

        public async Task<PerformanceReport> PerformanceReports(FilterationCriteria filterationCriteria)
        {
            var performanceReport = new PerformanceReport
            {
               PerformancePlan = []
            };

            try
            {
                if (filterationCriteria.StructureId != null)
                {
                    var BudgetYear = _dBContext.BudgetYears.Single(x => x.Year == filterationCriteria.BudgetYear);
                    var actualWorked = GetActualWorked(filterationCriteria, BudgetYear);

                    actualWorked = FilterActualWorked(filterationCriteria, actualWorked);

                    var quarterMonths = GenerateQuarterMonths(filterationCriteria);

                    performanceReport.PerformancePlan = CreatePerformancePlans(actualWorked, filterationCriteria.ReportType, quarterMonths);
                }
            }
            catch (Exception)
            {
                //TODO: Handle logs and exceptions
            }

            return performanceReport;
        }

        private List<ActivityProgress> GetActualWorked(FilterationCriteria filterationCriteria, BudgetYear BudgetYear)
        {
            return [.. _dBContext.ActivityProgresses
                .Include(x => x.Activity).ThenInclude(x => x.ActivityParent).ThenInclude(x => x.Task).ThenInclude(x => x.Plan).ThenInclude(x => x.Program)
                .Include(x => x.Activity).ThenInclude(x => x.Plan).ThenInclude(x => x.Program)
                .Include(x => x.Activity).ThenInclude(x => x.Task).ThenInclude(x => x.Plan).ThenInclude(x => x.Program)
                .Include(x => x.Quarter)
                .Where(x => x.CreatedAt >= BudgetYear.FromDate && x.CreatedAt <= BudgetYear.ToDate &&
                    (x.Activity.ActivityParentId != null ? x.Activity.ActivityParent.Task.Plan.StructureId :
                    (x.Activity.TaskId != null ? x.Activity.Task.Plan.StructureId : x.Activity.Plan.StructureId)) == filterationCriteria.StructureId)];
        }

        private List<ActivityProgress> FilterActualWorked(FilterationCriteria filterationCriteria, List<ActivityProgress> actualWorked)
        {
            if (filterationCriteria.FilterbyId == 1)
            {
                var dateRange = GetDateRangeForQuarter(filterationCriteria);
                actualWorked = actualWorked.Where(x => x.CreatedAt >= dateRange.begin && x.CreatedAt <= dateRange.end).ToList();
            }
            else if (filterationCriteria.FilterbyId == 2)
            {
                int month = filterationCriteria.Month ?? 1;
                int fromMonth = EthiopicDateTime.GetGregorianMonth(DateTime.Now.Day, month, DateTime.Now.Year);
                actualWorked = actualWorked.Where(x => x.CreatedAt.Month == fromMonth).ToList();
            }
            else if (filterationCriteria.FilterbyId == 3)
            {
                var dateRange = GetDateRangeFromCriteria(filterationCriteria);
                actualWorked = actualWorked.Where(x => x.CreatedAt.Date >= dateRange.begin.Date && x.CreatedAt.Date <= dateRange.end.Date).ToList();
            }

            return actualWorked;
        }

        private (DateTime begin, DateTime end) GetDateRangeForQuarter(FilterationCriteria filterationCriteria)
        {
            DateTime begin = DateTime.Now;
            DateTime end = DateTime.Now;

            switch (filterationCriteria.Quarter)
            {
                case 1:
                    begin = EthiopicDateTime.GetGregorianDate(1, 11, filterationCriteria.BudgetYear - 1);
                    end = EthiopicDateTime.GetGregorianDate(30, 1, filterationCriteria.BudgetYear);
                    break;
                case 2:
                    begin = EthiopicDateTime.GetGregorianDate(1, 2, filterationCriteria.BudgetYear);
                    end = EthiopicDateTime.GetGregorianDate(30, 4, filterationCriteria.BudgetYear);
                    break;
                case 3:
                    begin = EthiopicDateTime.GetGregorianDate(1, 5, filterationCriteria.BudgetYear);
                    end = EthiopicDateTime.GetGregorianDate(30, 7, filterationCriteria.BudgetYear);
                    break;
                case 4:
                    begin = EthiopicDateTime.GetGregorianDate(1, 8, filterationCriteria.BudgetYear);
                    end = EthiopicDateTime.GetGregorianDate(30, 10, filterationCriteria.BudgetYear);
                    break;
            }

            return (begin, end);
        }

        private (DateTime begin, DateTime end) GetDateRangeFromCriteria(FilterationCriteria filterationCriteria)
        {
            string[] fromDateParts = filterationCriteria.FromDate.Split('/');
            DateTime fromDate = EthiopicDateTime.GetGregorianDate(int.Parse(fromDateParts[1]), int.Parse(fromDateParts[0]), int.Parse(fromDateParts[2]));

            string[] toDateParts = filterationCriteria.ToDate.Split('/');
            DateTime toDate = EthiopicDateTime.GetGregorianDate(int.Parse(toDateParts[1]), int.Parse(toDateParts[0]), int.Parse(toDateParts[2]));

            
            return (fromDate, toDate);
        }

        private List<PerformancePlan> CreatePerformancePlans(List<ActivityProgress> actualWorked, string reportType, List<QuarterMonth> quarterMonths)
        {
            var performancePlans = new List<PerformancePlan>();

            foreach (var item in actualWorked)
            {
                var pp = new PerformancePlan
                {
                    ActualWorked = item.ActualWorked,
                    ReportDate = EthiopicDateTime.GetEthiopicDate(item.CreatedAt.Day, item.CreatedAt.Month, item.CreatedAt.Year),
                    ReportQuarter = item.Quarter.Order,
                    Target = item.Quarter.Target,
                    ActivityName = item.Activity.ActivityDescription ?? item.Activity.ActivityParent.ActivityParentDescription,
                    TaskName = item.Activity.ActivityParentId != null ? item.Activity.ActivityParent.Task.TaskDescription :
                                item.Activity.TaskId != null ? item.Activity.Task.TaskDescription : "--------",
                    PlanName = item.Activity.ActivityParentId != null ? item.Activity.ActivityParent.Task.Plan.PlanName :
                                item.Activity.TaskId != null ? item.Activity.Task.Plan.PlanName : item.Activity.Plan.PlanName,
                    ProgramName = item.Activity.ActivityParentId != null ? item.Activity.ActivityParent.Task.Plan.Program.ProgramName :
                                item.Activity.TaskId != null ? item.Activity.Task.Plan.Program.ProgramName : item.Activity.Plan.Program.ProgramName,
                    ActivityId = item.ActivityId
                };

                pp.Plannedtime = reportType == "Quarter" ?
                                quarterMonths[pp.ReportQuarter - 1].MonthName :
                                quarterMonths[pp.ReportQuarter - 1].MonthName;

                performancePlans.Add(pp);
            }

            return performancePlans;
        }

        public async Task<List<ActivityProgressViewModel>> GetActivityProgress(Guid? activityId)
        {

            var allProgresses = new List<ActivityProgressViewModel>();
            try
            {

                var allProgress = await
                     _dBContext.ActivityProgresses.
                     Include(x => x.ProgressAttachments).Where(x => x.ActivityId == activityId).OrderBy(x => x.CreatedAt)
                     .Select(progressRow => new ActivityProgressViewModel
                     {
                         ActivityId = progressRow.ActivityId,
                         Activity = progressRow.Activity,
                         ActualBudget = progressRow.ActualBudget,
                         ActualWorked = progressRow.ActualWorked,
                         DocumentPath = progressRow.FinanceDocumentPath,
                         EmployeeValue = progressRow.EmployeeValue,
                         EmployeeValueId = progressRow.EmployeeValueId,
                         Remark = progressRow.Remark,
                         Lat = float.Parse(progressRow.Lat),
                         Lng = float.Parse(progressRow.Lng),
                         CreatedDateTime = progressRow.CreatedAt,
                         ProgressAttachments = progressRow.ProgressAttachments

                     }).ToListAsync();


                return allProgress;
            }
            catch (Exception e)
            {
                return allProgresses;
            }
        }

        public Task<ProgresseReportByStructure> GetProgressByStructure(int budgetYear, Guid selectStructureId, string ReportBy){

            ProgresseReportByStructure progresseReportByStructure = new()
            {
                PlansLists = []
            };

            if (selectStructureId != null){

                var BudgetYear =  _dBContext.BudgetYears.Single(x => x.Year == budgetYear);
                progresseReportByStructure.PreviousBudgetYear = (BudgetYear.Year - 1).ToString();
                var allPlans = GetAllPlans(selectStructureId).Where(x => x.BudgetYearId == BudgetYear.Id).ToList();
                var processedPlans = ProcessPlans(allPlans, progresseReportByStructure, ReportBy);
                

            }
        }

        private List<PlansList> ProcessPlans(List<Plan> allPlans, ProgresseReportByStructure progresseReportByStructure, string ReportBy)
        {
            var plansList = new List<PlansList>();

            foreach (var plansItems in allPlans)
            {
                var plns = new PlansList
                {
                    PlanName = plansItems.PlanName,
                    Weight = plansItems.PlanWeight,
                    PlRemark = plansItems.Remark,
                    HasTask = plansItems.HasTask
                };

                if (plansItems.HasTask)
                {
                    plns.TaskLists = ProcessTasks(plansItems.Tasks, progresseReportByStructure, ReportBy);
                }

                plansList.Add(plns);
            }

            return plansList;
        }

        private List<TaskList> ProcessTasks(ICollection<Task> tasks, ProgresseReportByStructure progresseReportByStructure, string ReportBy)
        {
            var taskLsts = new List<TaskList>();

            foreach (var taskItems in tasks)
            {
                var taskLst = new TaskList
                {
                    TaskDescription = taskItems.TaskDescription,
                    TaskWeight = taskItems.Weight,
                    TRemark = taskItems.Remark,
                    HasActParent = taskItems.HasActivityParent
                };

                if (taskItems.HasActivityParent)
                {
                    taskLst.ActParentLst = ProcessActivityParents(taskItems.ActivitiesParents, progresseReportByStructure, ReportBy);
                }

                taskLsts.Add(taskLst);
            }

            return taskLsts;
        }

        private List<ActParentLst> ProcessActivityParents(ICollection<ActivityParent> activitiesParents, ProgresseReportByStructure progresseReportByStructure, string ReportBy)
        {
            var actParentLsts = new List<ActParentLst>();

            foreach (var actparentItems in activitiesParents)
            {
                var actparent = new ActParentLst
                {
                    ActParentDescription = actparentItems.ActivityParentDescription,
                    ActParentWeight = actparentItems.Weight,
                    ActpRemark = actparentItems.Remark
                };

                if (actparentItems.HasActivity)
                {
                    actparent.ActivityLists = ProcessActivities(actparentItems.Activities, progresseReportByStructure, ReportBy);
                }

                actParentLsts.Add(actparent);
            }

            return actParentLsts;
        }

        //TODO: use generic method and interface to avoid redundant methods such as the one below
        private void InitializeQuarterMonth(string reportBy, ref List<QuarterMonth> quarterMonth, ProgresseReportByStructure progresseReportByStructure)
        {
            int value = reportBy == "Quarter" ? 4 : 12;

            if (reportBy == "Quarter")
            {
                for (int i = 0; i < 4; i++)
                {
                    var quar = _dBContext.QuarterSettings.Single(x => x.QuarterOrder == i);
                    AdjustMonthRange(quar.StartMonth, quar.EndMonth);
                    
                    var mfi = new System.Globalization.DateTimeFormatInfo();
                    var fromG = mfi.GetMonthName(quar.StartMonth);
                    var toG = mfi.GetMonthName(quar.EndMonth);

                    var quarterMonths = new QuarterMonth
                    {
                        MonthName = "Quarter" + (i + 1)
                    };

                    quarterMonth.Add(quarterMonths);
                }

                progresseReportByStructure.PMINT = 4;
            }
            else
            {
                for (int i = 1; i <= 12; i++)
                {
                    int k = i >= 7 ? i - 6 : i + 6;

                    var quarterMonths = new QuarterMonth
                    {
                        MonthName = GetMonthName(k)
                    };

                    quarterMonth.Add(quarterMonths);
                }

                progresseReportByStructure.PMINT = 12;
            }

            progresseReportByStructure.PlanDurationInQuarter = quarterMonth;
        }

        private List<ActivityList> ProcessActivities(ICollection<Activity> activities, ProgresseReportByStructure progresseReportByStructure, string ReportBy)
        {
            var activityLsts = new List<ActivityList>();
            

            foreach (var ActItems in activities.Where(x => x.ActivityTargetDivisions != null))
            {
                var lst = new ActivityList
                {
                    ActivityDescription = ActItems.ActivityDescription,
                    Begining = ActItems.Begining,
                    MeasurementUnit = ActItems.UnitOfMeasurement.Name,
                    Target = ActItems.Goal,
                    Weight = ActItems.Weight,
                    Remark = ActItems.Remark,
                    ActualWorked = (float)Math.Round(ActItems.ActualWorked),
                    Progress = CalculateProgress(ActItems.ActualWorked, ActItems.Goal),
                    Plans = CalculatePlanAchievement(ReportBy, ActItems.ActivityTargetDivisions);
                };
                activityLsts.Add(lst);
            }

            return activityLsts;
        }

        private float CalculateProgress(float actualWorked, float target)
        {
            if (actualWorked > 0)
            {
                return actualWorked == target ? 100 : (float)Math.Round(actualWorked / target * 100, 2);
            }
            return 0;
        }

        private List<PlanAchievement> ProcessPlanAchievements(ProgresseReportByStructure progresseReportByStructure, string ReportBy)
        {
            var PlanAchievements = new List<PlanAchievement>();
            InitializeQuarterMonth(ReportBy, ref quarterMonth, progresseReportByStructure);
                  
        }

        private List<PlanAchievement> CalculatePlanAchievement(string reportBy, List<ActivityTargetDivision> byQuarter)
        {
            List<PlanAchievement> planOq = [];

            foreach (var itemQ in byQuarter)
            {
                var progresslist = _dBContext.ActivityProgresses.Where(x => x.QuarterId == itemQ.Id && x.QuarterId == itemQ.Id).ToList();
                PlanAchievement planO = new()
                {
                    Target = itemQ.Target,
                    Actual = progresslist.Sum(x => x.ActualWorked),
                    PercentageAchieved = itemQ.Target == 0 ? 0 : (float)Math.Round(planO.Actual / itemQ.Target * 100, 2)
                };
                planOq.Add(planO);
            }

            if (reportBy == "Quarter")
            {
                List<PlanAchievement> PlanAchievements = [];
                for (int i = 0; i < 12; i += 3)
                {
                    float targetSum = planOq[i].Target + planOq[i + 1].Target + planOq[i + 2].Target;
                    float actualSum = planOq[i].Actual + planOq[i + 1].Actual + planOq[i + 2].Actual;
                    PlanAchievement planO = new()
                    {
                        Target = targetSum,
                        Actual = actualSum
                    };
                    planO.PercentageAchieved = planO.Target == 0 ? 0 : (float)Math.Round(planO.Actual / planO.Target * 100, 2);
                    PlanAchievements.Add(planO);
                }
                return PlanAchievements;
            }
            else
            {
                return planOq;
            }
        }


        // private void InitializeQuarterMonth(string reportBy, ref List<QuarterMonth> quarterMonth, ProgresseReportByStructure reportStructure)
        // {
        //     if (reportStructure.PlanDuration == 0)
        //     {
        //         reportStructure.PlanDuration = reportBy == reporttype.Quarter.ToString() ? 4 : 12;
        //     }

        //     if (!quarterMonth.Any())
        //     {
        //         int value = reportBy == reporttype.Quarter.ToString() ? 4 : 12;
        //         if (reportBy == reporttype.Quarter.ToString())
        //         {
        //             for (int i = 0; i < 4; i++)
        //             {
        //                 var quar = _dBContext.QuarterSettings.Single(x => x.QuarterOrder == i);

        //                 quar.StartMonth = quar.StartMonth > 4 ? quar.StartMonth - 4 : quar.StartMonth + 8;
        //                 quar.EndMonth = quar.EndMonth > 4 ? quar.EndMonth - 4 : quar.EndMonth + 8;

        //                 System.Globalization.DateTimeFormatInfo mfi = new System.Globalization.DateTimeFormatInfo();
        //                 string fromG = mfi.GetMonthName(quar.StartMonth);
        //                 string toG = mfi.GetMonthName(quar.EndMonth);

        //                 QuarterMonth quarterMonths = new QuarterMonth
        //                 {
        //                     MonthName = "Quarter" + (i + 1)
        //                 };
        //                 quarterMonth.Add(quarterMonths);
        //             }
        //             reportStructure.PMINT = 4;
        //         }
        //         else
        //         {
        //             for (int i = 1; i <= 12; i++)
        //             {
        //                 int k = i >= 7 ? i - 6 : i + 6;

        //                 System.Globalization.DateTimeFormatInfo mfi = new System.Globalization.DateTimeFormatInfo();
        //                 string strMonthName = mfi.GetMonthName(k);

        //                 QuarterMonth quarterMonths = new QuarterMonth
        //                 {
        //                     MonthName = strMonthName
        //                 };
        //                 quarterMonth.Add(quarterMonths);
        //             }
        //             reportStructure.PMINT = 12;
        //         }
        //         reportStructure.PlanDuration2 = quarterMonth;
        //     }
        // }


        private List<Plan> GetAllPlans(Guid selectStructureId){

                var allPlans = _dBContext.Plans.Include(x => x.Program)
                      .Include(x => x.Tasks).ThenInclude(x => x.ActivitiesParents).ThenInclude(x => x.Activities).ThenInclude(x => x.UnitOfMeasurement)
                      .Include(x => x.Tasks).ThenInclude(x => x.Activities).ThenInclude(x => x.UnitOfMeasurement)
                      .Include(x => x.Activities).ThenInclude(x => x.UnitOfMeasurement)
                      .Include(x => x.Tasks).ThenInclude(x => x.ActivitiesParents).ThenInclude(x => x.Activities).ThenInclude(x => x.ActivityTargetDivisions)
                      .Include(x => x.Tasks).ThenInclude(x => x.Activities).ThenInclude(x => x.ActivityTargetDivisions)
                      .Include(x => x.Activities).ThenInclude(x => x.ActivityTargetDivisions)
                      .Where(x => x.StructureId == selectStructureId)
                      .OrderBy(c => c.PeriodStartAt)
                      .ToList();

                return allPlans;
        }

        public async Task<List<EstimatedCostDto>> GetEstimatedCost(Guid structureId, int budgetYear)
        {
            var plans = await GetPlans(structureId, budgetYear);
            var estimatedCosts = new List<EstimatedCostDto>();

            foreach (var item in plans)
            {
                if (item.Activities.Any())
                {
                    var estimatedCost = CalculateEstimatedCostForActivities(item.Activities.First());
                    estimatedCosts.Add(estimatedCost);
                }
                else
                {
                    var estimatedCost = new EstimatedCostDto
                    {
                        Description = item.PlanName,
                        Tasks = new List<EstimatedCostDto>()
                    };

                    foreach (var taskItem in item.Tasks)
                    {
                        var taskEstimatedCost = CalculateEstimatedCostForTask(taskItem);
                        estimatedCost.Tasks.Add(taskEstimatedCost);
                    }

                    estimatedCosts.Add(estimatedCost);
                }
            }

            return estimatedCosts;
        }

        private async Task<List<Plan>> GetPlans(Guid structureId, int budgetYear)
        {
            return await _dBContext.Plans
                .Include(x => x.Tasks).ThenInclude(x => x.ActivitiesParents).ThenInclude(x => x.Activities).ThenInclude(x => x.ActProgress)
                .Include(x => x.Tasks).ThenInclude(x => x.Activities).ThenInclude(x => x.ActProgress)
                .Include(x => x.Activities).ThenInclude(x => x.ActProgress)
                .Include(x => x.Tasks).ThenInclude(x => x.ActivitiesParents).ThenInclude(x => x.Activities).ThenInclude(x => x.ActivityTargetDivisions)
                .Include(x => x.Tasks).ThenInclude(x => x.Activities).ThenInclude(x => x.ActivityTargetDivisions)
                .Include(x => x.Activities).ThenInclude(x => x.ActivityTargetDivisions)
                .Where(x => x.StructureId == structureId && x.BudgetYear.Year == budgetYear)
                .ToListAsync();
        }

        private EstimatedCostDto CalculateEstimatedCostForActivities(Activity activity)
        {
            var estimatedCost = new EstimatedCostDto
            {
                Description = activity.ActivityDescription,
                PlannedBudjet = $"{activity.PlanedBudget} ETB",
                ActualBudget = $"{activity.ActualBudget} ETB",
                BudgetVariance = $"{activity.PlanedBudget - activity.ActualBudget} ETB"
            };

            var planVariance = GetPlanDateVariance(activity.ShouldStart, activity.ShouldEnd);
            estimatedCost.BudgetHours = planVariance[2];

            if (activity.ActualStart.HasValue && activity.ActualEnd.HasValue)
            {
                var actualVariance = GetActualDateVariance(Convert.ToInt32(planVariance[0]), Convert.ToInt32(planVariance[1]), activity.ShouldStart, activity.ShouldEnd);
                estimatedCost.ActualHours = actualVariance[0];
                estimatedCost.HourVariance = actualVariance[1];
            }

            return estimatedCost;
        }

        private EstimatedCostDto CalculateEstimatedCostForTask(Task task)

        {
            var taskEstimatedCost = new EstimatedCostDto
            {
                Description = task.TaskDescription,
                Tasks = new List<EstimatedCostDto>()
            };

            foreach (var parentActivity in task.ActivitiesParents)
            {
                var parentEstimatedCost = new EstimatedCostDto
                {
                    Description = parentActivity.ActivityParentDescription,
                    Tasks = new List<EstimatedCostDto>()
                };

                foreach (var activity in parentActivity.Activities)
                {
                    var activityEstimatedCost = CalculateEstimatedCostForActivities(activity);
                    parentEstimatedCost.Tasks.Add(activityEstimatedCost);
                }

                taskEstimatedCost.Tasks.Add(parentEstimatedCost);
            }

            foreach (var activity in task.Activities)
            {
                var activityEstimatedCost = CalculateEstimatedCostForActivities(activity);
                taskEstimatedCost.Tasks.Add(activityEstimatedCost);
            }

            return taskEstimatedCost;
        }

        static string[] GetPlanDateVariance(DateTime startDate, DateTime endDate)
        {
            DateTime now3 = startDate.Date;
            DateTime newyear3 = endDate.Date;
            var totaldates = " ";
            var totalmonths = Math.Abs((newyear3.Year - now3.Year) * 12 + newyear3.Month - now3.Month);
            totalmonths += newyear3.Day < now3.Day ? -1 : 0;
            var years = totalmonths / 12;
            var months = totalmonths % 12;
            var days = Math.Abs(newyear3.Subtract(now3.AddMonths(totalmonths)).Days);
            totaldates += years > 0 ? years + " " + "year" : months > 0 ? " " + months + " Month " : " ";
            totaldates += days + " days";
            string[] her = new string[3];
            her[0] = days.ToString();
            her[1] = totalmonths.ToString();
            her[2] = totaldates;
            return her;
        }

        static string[] GetActualDateVariance(int days, int totalmonths, DateTime startDate, DateTime endDate)
        {
            var totalActualMonth = (endDate.Year - startDate.Year) * 12 + endDate.Month - startDate.Month;
            totalActualMonth += endDate.Day < startDate.Day ? -1 : 0;

            var ActualYears = totalActualMonth / 12;
            var ActualMonth = totalActualMonth % 12;
            var ActualDays = endDate.Subtract(startDate.AddMonths(totalActualMonth)).Days;
            var Difference = totalmonths - totalActualMonth;
            var DifferenceYears = Difference / 12;
            var DifferenceMonth = Difference % 12;
            var DifferenceDays = days - ActualDays;
            var TotalActualDate = ActualYears > 0 ? ActualYears + " year" : ActualMonth > 0 ? " " + ActualMonth + " Month" : " ";
            TotalActualDate += ActualDays + " " + " days";
            var VarianceDate = DifferenceYears > 0 ? DifferenceYears + " " + " year" : DifferenceMonth > 0 ? " " + DifferenceMonth + " Month" : " ";
            VarianceDate += DifferenceDays + " " + "days";

            string[] her = new string[2];
            her[0] = TotalActualDate;
            her[1] = VarianceDate;
            return her;
        }
    }
}
