using Microsoft.EntityFrameworkCore;
using PM_Case_Managemnt_Implementation.DTOS.Common;
using PM_Case_Managemnt_Implementation.Helpers;
using PM_Case_Managemnt_Infrustructure.Data;
using PM_Case_Managemnt_Infrustructure.Models.Common;
using PM_Case_Managemnt_Infrustructure.Models.PM;

namespace PM_Case_Managemnt_Implementation.Services.PM.ProgressReport
{
    public class ProgressReportService : IProgressReportService
    {

        private readonly DBContext _dBContext;
        public ProgressReportService(DBContext context)
        {
            _dBContext = context;
        }


        public async Task<List<DiagramDto>> GetDirectorLevelPerformance(Guid subOrgId, Guid? BranchId)
        {

            var orgStructures = _dBContext.OrganizationalStructures.Where(x => x.SubsidiaryOrganizationId == subOrgId).Include(x => x.ParentStructure).ToList();
            var orgStructureIds = orgStructures.Select(x => x.Id).ToList();

            var employess = _dBContext.Employees.Where(x => orgStructureIds.Contains(x.OrganizationalStructureId)).ToList();
            var childs = new List<DiagramDto>();

            var parentStructure = orgStructures.FirstOrDefault(x => x.ParentStructureId == null);

            var programBudgetYearsIds = _dBContext.ProgramBudgetYears.Where(x => x.SubsidiaryOrganizationId == subOrgId).Select(x => x.Id).ToList();

            var BudgetYear = _dBContext.BudgetYears.Where(x => programBudgetYearsIds.Contains(x.ProgramBudgetYearId)).Single(x => x.RowStatus == RowStatus.Active);




            var DiagramDro = new DiagramDto()
            {


                data = new
                {
                    name = parentStructure.StructureName,
                    weight = GetContribution(parentStructure.Id, parentStructure.Weight, BudgetYear.Id),
                    head = employess.FirstOrDefault(x => x.OrganizationalStructureId == parentStructure.Id && x.Position == Position.Director)?.Title + " " +
                                           employess.FirstOrDefault(x => x.OrganizationalStructureId == parentStructure.Id && x.Position == Position.Director)?.FullName

                },
                label = parentStructure.StructureName,
                expanded = true,
                type = "organization",
                styleClass = GetContribution(parentStructure.Id, parentStructure.Weight, BudgetYear.Id) < 25.00 ? "bg-danger text-white" : "bg-success text-white",
                id = parentStructure.Id,
                order = parentStructure.Order,
                children = new List<DiagramDto>()

            };

            childs.Add(DiagramDro);

            var remainingStractures = orgStructures.Where(x => x.ParentStructureId != null).OrderBy(x => x.Order).Select(x => x.ParentStructureId).Distinct();

            foreach (var items in remainingStractures)
            {
                var children = orgStructures.Where(x => x.ParentStructureId == items).Select(x => new DiagramDto
                {
                    data = new
                    {
                        name = x.StructureName,
                        weight = GetContribution(x.Id, x.Weight, BudgetYear.Id),
                        head = employess.FirstOrDefault(x => x.OrganizationalStructureId == x.Id && x.Position == Position.Director)?.Title + " " +
                                       employess.FirstOrDefault(x => x.OrganizationalStructureId == x.Id && x.Position == Position.Director)?.FullName

                    },

                    label = x.StructureName,
                    expanded = true,
                    type = "organization",
                    styleClass = GetContribution(x.Id, x.Weight, BudgetYear.Id) < 25.00 ? "bg-danger text-white" : "bg-success text-white",
                    id = x.Id,
                    parentId = x.ParentStructureId,
                    order = x.Order,
                    children = new List<DiagramDto>()
                }).ToList();


                childs.AddRange(children);


            }
            for (var j = childs.Max(x => x.order); j >= 0; j--)
            {
                var childList = childs.Where(x => x.order == j).ToList();

                float weight = (float)childList.Sum(x => (float)x.data?.weight);

                foreach (var item in childList)
                {

                    var org = childs.FirstOrDefault(x => x.id == item.parentId);


                    if (org != null)
                    {
                        org.children.Add(item);
                    }


                }
            }
            List<DiagramDto> result = new List<DiagramDto>();

            if (childs.Any())
            {
                result.Add(childs[0]);
            }
            return result;



        }


        public async Task<PlanReportByProgramDto> PlanReportByProgram(Guid subOrgId, string BudgetYear, string ReportBy)
        {
            PlanReportByProgramDto prbp = new PlanReportByProgramDto();

            List<ProgramViewModel> ProgramViewModelList = new List<ProgramViewModel>();
            try
            {
                if (BudgetYear != null)
                {

                    int budgetYearPlan = Convert.ToInt32(BudgetYear);
                    var BudgetYearValue = _dBContext.BudgetYears.Single(x => x.Year == budgetYearPlan);
                    var ProgLists = _dBContext.Programs.Where(x => x.ProgramBudgetYearId == BudgetYearValue.ProgramBudgetYearId && x.SubsidiaryOrganizationId == subOrgId).OrderBy(x => x.CreatedAt).ToList();
                    string MeasurementName = "";
                    foreach (var items in ProgLists)
                    {
                        ProgramViewModel programView = new ProgramViewModel();
                        programView.ProgramName = items.ProgramName;
                        programView.ProgramPlanViewModels = new List<ProgramPlanViewModel>();
                        var plansLst = _dBContext.Plans.Where(x => x.ProgramId == items.Id).OrderBy(x => x.CreatedAt).ToList();
                        foreach (var planItems in plansLst)
                        {
                            ProgramPlanViewModel ProgramPlanData = new ProgramPlanViewModel();
                            ProgramPlanData.FiscalPlanPrograms = new List<FiscalPlanProgram>();
                            ProgramPlanData.ProgramName = items.ProgramName;
                            if (!planItems.HasTask)
                            {
                                var PlanActivities = _dBContext.Activities.Include(x => x.UnitOfMeasurement).SingleOrDefault(x => x.PlanId == planItems.Id);
                                if (PlanActivities != null)
                                {
                                    ProgramPlanData.MeasurementUnit = PlanActivities.UnitOfMeasurement.Name;
                                    ProgramPlanData.PlanNAme = planItems.PlanName;



                                    ProgramPlanData.TotalBirr = planItems.PlandBudget;
                                    ProgramPlanData.TotalGoal = planItems.Activities.Sum(x => x.Goal);
                                    var TargetActivities = _dBContext.ActivityTargetDivisions.Where(x => x.ActivityId == PlanActivities.Id).OrderBy(x => x.Order).ToList();
                                    foreach (var tarItems in TargetActivities)
                                    {
                                        FiscalPlanProgram progs = new FiscalPlanProgram();
                                        progs.RowORder = tarItems.Order;
                                        progs.FinancialValue = tarItems.TargetBudget;
                                        progs.fisicalValue = tarItems.Target;
                                        ProgramPlanData.FiscalPlanPrograms.Add(progs);
                                    }
                                }

                                programView.ProgramPlanViewModels.Add(ProgramPlanData);
                            }
                            else
                            {

                                var TasksOnPlan = _dBContext.Tasks.Where(x => x.PlanId == planItems.Id).OrderBy(x => x.CreatedAt).ToList();


                                float TotalBirr = 0;

                                List<FiscalPlanProgram> fsForPlan = new List<FiscalPlanProgram>();
                                foreach (var taskitems in TasksOnPlan)
                                {
                                    if (taskitems.HasActivityParent == false)
                                    {

                                        var TaskActivities = _dBContext.Activities.Include(x => x.UnitOfMeasurement).FirstOrDefault(x => x.TaskId == taskitems.Id);
                                        if (TaskActivities != null)
                                        {


                                            TotalBirr += taskitems.PlanedBudget;
                                            if (MeasurementName == "")
                                            {
                                                MeasurementName = TaskActivities.UnitOfMeasurement.Name;
                                            }
                                            var TargetForTasks = _dBContext.ActivityTargetDivisions.Include(x => x.Activity).Include(x => x.Activity).ThenInclude(x => x.Task).Where(x => x.ActivityId == TaskActivities.Id).OrderBy(x => x.Order).ToList();
                                            foreach (var tarItems in TargetForTasks)
                                            {
                                                if (!fsForPlan.Where(x => x.RowORder == tarItems.Order).Any())
                                                {
                                                    FiscalPlanProgram progs = new FiscalPlanProgram();
                                                    progs.RowORder = tarItems.Order;
                                                    progs.FinancialValue = tarItems.TargetBudget;
                                                    progs.fisicalValue = (tarItems.Target * tarItems.Activity.Weight) / _dBContext.Activities.Where(x => x.TaskId == tarItems.Activity.TaskId).Sum(x => x.Weight);
                                                    //taskwe (float)tarItems.Activity.Task.Weight
                                                    progs.fisicalValue = (UInt32)Math.Round(progs.fisicalValue, 2);

                                                    fsForPlan.Add(progs);
                                                }
                                                else
                                                {
                                                    var AddFT = fsForPlan.Where(x => x.RowORder == tarItems.Order).FirstOrDefault();
                                                    AddFT.FinancialValue += tarItems.TargetBudget;
                                                    AddFT.fisicalValue += (tarItems.Target * tarItems.Activity.Weight) / _dBContext.Activities.Where(x => x.TaskId == tarItems.Activity.TaskId).Sum(x => x.Weight);

                                                }

                                            }
                                        }
                                    }
                                    else
                                    {

                                        var ParentActivities = _dBContext.ActivityParents.Where(x => x.TaskId == taskitems.Id).ToList();


                                        TotalBirr += (float)ParentActivities.Sum(x => x.PlanedBudget);
                                        foreach (var pAct in ParentActivities)
                                        {
                                            foreach (var SubAct in pAct.Activities)
                                            {
                                                var TargetForTasks = _dBContext.ActivityTargetDivisions.Include(x => x.Activity).Include(x => x.Activity).ThenInclude(x => x.ActivityParent).Where(x => x.ActivityId == SubAct.Id).OrderBy(x => x.Order).ToList();
                                                foreach (var tarItems in TargetForTasks)
                                                {
                                                    if (!fsForPlan.Where(x => x.RowORder == tarItems.Order).Any())
                                                    {
                                                        FiscalPlanProgram progs = new FiscalPlanProgram();
                                                        progs.RowORder = tarItems.Order;
                                                        progs.FinancialValue = tarItems.TargetBudget;
                                                        progs.fisicalValue = (tarItems.Target * tarItems.Activity.Weight) / (float)tarItems.Activity.ActivityParent.Task.Weight;
                                                        progs.fisicalValue = (UInt32)Math.Round(progs.fisicalValue, 2);

                                                        fsForPlan.Add(progs);
                                                    }
                                                    else
                                                    {
                                                        var AddFT = fsForPlan.Where(x => x.RowORder == tarItems.Order).FirstOrDefault();
                                                        AddFT.FinancialValue += tarItems.TargetBudget == 0 ? 0 : tarItems.TargetBudget;
                                                        AddFT.fisicalValue += (tarItems.Target * tarItems.Activity.Weight) / (float)tarItems.Activity.ActivityParent.Task.Weight;
                                                        AddFT.fisicalValue = (UInt32)Math.Round(AddFT.fisicalValue, 2);

                                                    }
                                                }
                                            }
                                        }
                                    }
                                }

                                ProgramPlanData.PlanNAme = planItems.PlanName;
                                ProgramPlanData.ProgramName = items.ProgramName;

                                ProgramPlanData.TotalBirr = TotalBirr;
                                ProgramPlanData.TotalGoal = fsForPlan.Sum(x => x.fisicalValue);
                                ProgramPlanData.MeasurementUnit = MeasurementName;
                                ProgramPlanData.FiscalPlanPrograms.AddRange(fsForPlan);
                                programView.ProgramPlanViewModels.Add(ProgramPlanData);
                            }
                        }
                        ProgramViewModelList.Add(programView);
                    }

                    var MonthDeclarator = ProgramViewModelList.Where(x => x.ProgramPlanViewModels.Count() > 0).First().ProgramPlanViewModels.Where(x => x.FiscalPlanPrograms.Count > 0).First().FiscalPlanPrograms.ToList();


                    if (MonthDeclarator.Any())
                    {
                        if (ReportBy == "Quarter")
                        {
                            int j = 0;
                            for (int i = 0; i < 4; i++)
                            {
                                j = j + 3;
                                foreach (var progs in ProgramViewModelList)
                                {
                                    foreach (var plns in progs.ProgramPlanViewModels)
                                    {
                                        var newMonth = plns.FiscalPlanPrograms.Where(x => x.RowORder <= j && x.MonthName == null);
                                        FiscalPlanProgram planProgram = new FiscalPlanProgram();
                                        planProgram.RowORder = i + 1;
                                        planProgram.MonthName = "Quarter" + " " + planProgram.RowORder;
                                        planProgram.fisicalValue = newMonth.Sum(x => x.fisicalValue);
                                        planProgram.FinancialValue = newMonth.Sum(x => x.FinancialValue);
                                        plns.FiscalPlanPrograms.Add(planProgram);
                                        plns.FiscalPlanPrograms.RemoveRange(0, newMonth.Count());
                                    }
                                }

                            }
                            MonthDeclarator = ProgramViewModelList.Where(x => x.ProgramPlanViewModels.Count() > 0).First().ProgramPlanViewModels.Where(x => x.FiscalPlanPrograms != null).First().FiscalPlanPrograms.ToList();
                        }
                        else
                        {
                            int newI = 0;
                            for (int i = 1; i <= MonthDeclarator.Count(); i++)
                            {
                                int h = 0;
                                if (i >= 7)
                                {
                                    h = i - 6;
                                }

                                else
                                {
                                    h = i + 6;
                                }

                                System.Globalization.DateTimeFormatInfo mfi = new
                                    System.Globalization.DateTimeFormatInfo();
                                string strMonthName = mfi.GetMonthName(h).ToString();
                                MonthDeclarator.Find(x => x.RowORder == i);
                                MonthDeclarator[newI].MonthName = strMonthName;
                                newI++;
                            }
                        }


                    }


                    prbp.MonthCounts = MonthDeclarator.ToList();

                }


                prbp.ProgramViewModels = ProgramViewModelList.ToList();




                return prbp;
            }
            catch (Exception e)
            {

                return prbp;
            }



        }

        public async Task<PlanReportDetailDto> StructureReportByProgram(string BudgetYear, string ProgramId, string ReportBy)
        {
            PlanReportDetailDto planReportDetailDto = new PlanReportDetailDto();
            try
            {


                if (BudgetYear != null)
                {
                    var MonthDeclarator = new List<ActivityTargetDivisionReport>();
                    List<QuarterMonth> qm = new List<QuarterMonth>();
                    List<ProgramWithStructure> progWithStructure = new List<ProgramWithStructure>();
                    Guid newProgram = Guid.Parse(ProgramId);
                    var StrucsinProg = _dBContext.Plans.Where(x => x.ProgramId == newProgram).Select(x => x.StructureId).Distinct();
                    var Structures = _dBContext.OrganizationalStructures.Where(x => StrucsinProg.Contains(x.Id)).ToList();
                    foreach (var stItems in Structures)
                    {
                        ProgramWithStructure progwithStu = new ProgramWithStructure();
                        progwithStu.StrutureName = stItems.StructureName;
                        progwithStu.StructurePlans = new List<StructurePlan>();
                        var plansinStruc = _dBContext.Plans
                            .Include(x => x.Tasks).ThenInclude(x => x.ActivitiesParents).ThenInclude(x => x.Activities).ThenInclude(x => x.UnitOfMeasurement)
                            .Include(x => x.Tasks).ThenInclude(x => x.Activities).ThenInclude(x => x.UnitOfMeasurement)
                            .Include(x => x.Activities).ThenInclude(x => x.UnitOfMeasurement)
                            .Include(x => x.Tasks).ThenInclude(x => x.ActivitiesParents).ThenInclude(x => x.Activities).ThenInclude(x => x.ActivityTargetDivisions)
                            .Include(x => x.Tasks).ThenInclude(x => x.Activities).ThenInclude(x => x.ActivityTargetDivisions)
                            .Include(x => x.Activities).ThenInclude(x => x.ActivityTargetDivisions)
                            .Where(x => x.StructureId == stItems.Id && x.ProgramId == newProgram).ToList().OrderBy(x => x.CreatedAt);
                        foreach (var Plitems in plansinStruc)
                        {
                            StructurePlan StrPlan = new StructurePlan();
                            StrPlan.PlanName = Plitems.PlanName;
                            if (Plitems.HasTask)
                            {
                                StrPlan.PlanTasks = new List<PlanTask>();
                                foreach (var taskPl in Plitems.Tasks.OrderBy(x => x.CreatedAt))
                                {
                                    PlanTask pT = new PlanTask();
                                    pT.TaskName = taskPl.TaskDescription;
                                    if (taskPl.HasActivityParent)
                                    {
                                        pT.TaskActivities = new List<TaskActivity>();
                                        var ActInTask = taskPl.ActivitiesParents.ToList().OrderBy(x => x.CreatedAt);
                                        foreach (var parentAct in ActInTask)
                                        {
                                            TaskActivity taskActivity = new TaskActivity();
                                            taskActivity.ActivityName = parentAct.ActivityParentDescription;
                                            if (parentAct.HasActivity)
                                            {
                                                taskActivity.ActSubActivity = new List<ActSubActivity>();
                                                foreach (var subActs in parentAct.Activities.OrderBy(x => x.CreatedAt))
                                                {
                                                    ActSubActivity actSub = new ActSubActivity();
                                                    actSub.SubActivityDescription = subActs.ActivityDescription;
                                                    if (actSub != null)
                                                    {
                                                        actSub.Weight = subActs.Weight;
                                                        actSub.Target = subActs.Goal;
                                                        actSub.UnitOfMeasurement = subActs.UnitOfMeasurement.Name;
                                                        actSub.subActivityTargetDivision = new List<ActivityTargetDivisionReport>();
                                                        foreach (var tp in subActs.ActivityTargetDivisions)
                                                        {
                                                            ActivityTargetDivisionReport PTar = new ActivityTargetDivisionReport();
                                                            PTar.Order = tp.Order;
                                                            PTar.TargetValue = tp.Target;
                                                            actSub.subActivityTargetDivision.Add(PTar);
                                                            if (!MonthDeclarator.Where(x => x.Order == tp.Order).Any())
                                                            {
                                                                MonthDeclarator.Add(PTar);
                                                            }

                                                        }
                                                    }

                                                    taskActivity.ActSubActivity.Add(actSub);
                                                }
                                            }
                                            else
                                            {
                                                taskActivity.ActivityTargetDivision = new List<ActivityTargetDivisionReport>();
                                                var PActTarDiv = parentAct.Activities.FirstOrDefault();
                                                if (PActTarDiv != null)
                                                {
                                                    taskActivity.Weight = PActTarDiv.Weight;
                                                    taskActivity.UnitOfMeasurement = PActTarDiv.UnitOfMeasurement.Name;
                                                    taskActivity.Target = PActTarDiv.Goal;
                                                    foreach (var tp in PActTarDiv.ActivityTargetDivisions)
                                                    {
                                                        ActivityTargetDivisionReport PTar = new ActivityTargetDivisionReport();
                                                        PTar.Order = tp.Order;
                                                        PTar.TargetValue = tp.Target;
                                                        taskActivity.ActivityTargetDivision.Add(PTar);
                                                        if (!MonthDeclarator.Where(x => x.Order == tp.Order).Any())
                                                        {
                                                            MonthDeclarator.Add(PTar);
                                                        }
                                                    }
                                                }
                                            }
                                            pT.TaskActivities.Add(taskActivity);
                                        }
                                    }
                                    else
                                    {
                                        pT.TaskTargetDivision = new List<ActivityTargetDivisionReport>();
                                        var tasTarDiv = taskPl.Activities.FirstOrDefault();
                                        if (tasTarDiv != null)
                                        {
                                            pT.Weight = tasTarDiv.Weight;
                                            pT.UnitOfMeasurement = tasTarDiv.UnitOfMeasurement.Name;
                                            pT.Target = tasTarDiv.Goal;
                                            foreach (var tp in tasTarDiv.ActivityTargetDivisions)
                                            {
                                                ActivityTargetDivisionReport PTar = new ActivityTargetDivisionReport();
                                                PTar.Order = tp.Order;
                                                PTar.TargetValue = tp.Target;
                                                pT.TaskTargetDivision.Add(PTar);
                                                if (!MonthDeclarator.Where(x => x.Order == tp.Order).Any())
                                                {
                                                    MonthDeclarator.Add(PTar);
                                                }
                                            }
                                        }
                                    }
                                    StrPlan.PlanTasks.Add(pT);
                                }
                            }
                            else
                            {
                                StrPlan.PlanTargetDivision = new List<ActivityTargetDivisionReport>();
                                var targetDiv = Plitems.Activities.FirstOrDefault();
                                if (targetDiv != null)
                                {
                                    StrPlan.Weight = targetDiv.Weight;
                                    StrPlan.UnitOfMeasurement = targetDiv.UnitOfMeasurement.Name;
                                    StrPlan.Target = targetDiv.Goal;
                                    foreach (var tP in targetDiv.ActivityTargetDivisions)
                                    {

                                        ActivityTargetDivisionReport PTar = new ActivityTargetDivisionReport();
                                        PTar.Order = tP.Order;
                                        PTar.TargetValue = tP.Target;
                                        StrPlan.PlanTargetDivision.Add(PTar);
                                        if (!MonthDeclarator.Where(x => x.Order == tP.Order).Any())
                                        {
                                            MonthDeclarator.Add(PTar);
                                        }
                                    }
                                }
                            }

                            progwithStu.StructurePlans.Add(StrPlan);
                        }

                        progWithStructure.Add(progwithStu);
                    }


                    if (MonthDeclarator.Any())
                    {
                        if (ReportBy == "Quarter")
                        {
                            MonthDeclarator = new List<ActivityTargetDivisionReport>();
                            int j = 0;
                            for (int i = 0; i < 4; i++)
                            {
                                j = j + 3;
                                foreach (var stu in progWithStructure)
                                {
                                    foreach (var progs in stu.StructurePlans)
                                    {
                                        if (progs.PlanTargetDivision != null)
                                        {
                                            var newMonth = progs.PlanTargetDivision.Where(x => x.Order <= j && x.MonthName == null);
                                            ActivityTargetDivisionReport planProgram = new ActivityTargetDivisionReport();
                                            planProgram.Order = i + 1;
                                            planProgram.MonthName = "Quarter" + " " + planProgram.Order;
                                            planProgram.TargetValue = newMonth.Sum(x => x.TargetValue);
                                            progs.PlanTargetDivision.Add(planProgram);
                                            progs.PlanTargetDivision.RemoveRange(0, newMonth.Count());
                                            if (!MonthDeclarator.Where(x => x.Order == planProgram.Order).Any())
                                            {
                                                MonthDeclarator.Add(planProgram);
                                            }
                                        }
                                        else
                                        {
                                            foreach (var task in progs.PlanTasks)
                                            {
                                                if (task.TaskTargetDivision != null)
                                                {
                                                    var newMonthTask = task.TaskTargetDivision.Where(x => x.Order <= j && x.MonthName == null);
                                                    ActivityTargetDivisionReport taskProg = new ActivityTargetDivisionReport();
                                                    taskProg.Order = i + 1;
                                                    taskProg.MonthName = "Quarter" + " " + taskProg.Order;
                                                    taskProg.TargetValue = newMonthTask.Sum(x => x.TargetValue);
                                                    task.TaskTargetDivision.Add(taskProg);
                                                    task.TaskTargetDivision.RemoveRange(0, newMonthTask.Count());
                                                    if (!MonthDeclarator.Where(x => x.Order == taskProg.Order).Any())
                                                    {
                                                        MonthDeclarator.Add(taskProg);
                                                    }
                                                }
                                                else
                                                {
                                                    foreach (var activ in task.TaskActivities)
                                                    {
                                                        if (activ.ActivityTargetDivision != null)
                                                        {
                                                            var newMonthAct = activ.ActivityTargetDivision.Where(x => x.Order <= j && x.MonthName == null);
                                                            ActivityTargetDivisionReport taskProg = new ActivityTargetDivisionReport();
                                                            taskProg.Order = i + 1;
                                                            taskProg.MonthName = "Quarter" + " " + taskProg.Order;
                                                            taskProg.TargetValue = newMonthAct.Sum(x => x.TargetValue);
                                                            activ.ActivityTargetDivision.Add(taskProg);
                                                            activ.ActivityTargetDivision.RemoveRange(0, newMonthAct.Count());
                                                            if (!MonthDeclarator.Where(x => x.Order == taskProg.Order).Any())
                                                            {
                                                                MonthDeclarator.Add(taskProg);
                                                            }
                                                        }
                                                        else
                                                        {
                                                            foreach (var subAct in activ.ActSubActivity)
                                                            {
                                                                if (subAct.subActivityTargetDivision != null)
                                                                {
                                                                    var newMonthSAct = subAct.subActivityTargetDivision.Where(x => x.Order <= j && x.MonthName == null);
                                                                    ActivityTargetDivisionReport taskProg = new ActivityTargetDivisionReport();
                                                                    taskProg.Order = i + 1;
                                                                    taskProg.MonthName = "Quarter" + " " + taskProg.Order;
                                                                    taskProg.TargetValue = newMonthSAct.Sum(x => x.TargetValue);
                                                                    subAct.subActivityTargetDivision.Add(taskProg);
                                                                    subAct.subActivityTargetDivision.RemoveRange(0, newMonthSAct.Count());
                                                                    if (!MonthDeclarator.Where(x => x.Order == taskProg.Order).Any())
                                                                    {
                                                                        MonthDeclarator.Add(taskProg);
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }

                                        }

                                    }
                                }
                            }

                        }
                        else
                        {
                            int newI = 0;
                            for (int i = 1; i <= MonthDeclarator.Count(); i++)
                            {
                                int h = 0;
                                if (i >= 7)
                                {
                                    h = i - 6;
                                }

                                else
                                {
                                    h = i + 6;
                                }

                                System.Globalization.DateTimeFormatInfo mfi = new
                                    System.Globalization.DateTimeFormatInfo();
                                string strMonthName = mfi.GetMonthName(h).ToString();
                                MonthDeclarator.Find(x => x.Order == i);
                                MonthDeclarator[newI].MonthName = strMonthName;
                                newI++;
                            }
                        }


                    }


                    planReportDetailDto.MonthCounts = MonthDeclarator.ToList();
                    planReportDetailDto.ProgramWithStructure = progWithStructure.ToList();
                }

                return planReportDetailDto;



            }
            catch (Exception e)
            {
                return planReportDetailDto;
            }


        }

        public async Task<PlannedReport> PlanReports(string BudgetYea, Guid selectStructureId, string ReportBy)
        {

            PlannedReport plannedReport = new PlannedReport();

            try
            {

                var BudgetYear = _dBContext.BudgetYears.Single(x => x.Year.ToString() == BudgetYea);

                var allPlans = _dBContext.Plans
                      .Include(x => x.Tasks).ThenInclude(x => x.ActivitiesParents).ThenInclude(x => x.Activities).ThenInclude(x => x.UnitOfMeasurement)
                                 .Include(x => x.Tasks).ThenInclude(x => x.Activities).ThenInclude(x => x.UnitOfMeasurement)
                                 .Include(x => x.Activities).ThenInclude(x => x.UnitOfMeasurement)
                                 .Include(x => x.Tasks).ThenInclude(x => x.ActivitiesParents).ThenInclude(x => x.Activities).ThenInclude(x => x.ActivityTargetDivisions)
                                 .Include(x => x.Tasks).ThenInclude(x => x.Activities).ThenInclude(x => x.ActivityTargetDivisions)
                                 .Include(x => x.Activities).ThenInclude(x => x.ActivityTargetDivisions)

                     .Where(x => x.StructureId == selectStructureId && x.BudgetYearId == BudgetYear.Id).OrderBy(c => c.CreatedAt).ToList();

                List<PlansLst> plansLsts = new List<PlansLst>();

                List<QuarterMonth> QuarterMonth = new List<QuarterMonth>();

                foreach (var plansItems in allPlans)
                {
                    PlansLst plns = new PlansLst();
                    plns.PlanName = plansItems.PlanName;
                    plns.Weight = plansItems.PlanWeight;
                    plns.PlRemark = plansItems.Remark;
                    plns.HasTask = plansItems.HasTask;
                    if (plansItems.HasTask)
                    {
                        List<TaskLst> taskLsts = new List<TaskLst>();

                        var Taskes = plansItems.Tasks.OrderBy(x => x.CreatedAt);
                        foreach (var taskItems in Taskes)
                        {
                            TaskLst taskLst = new TaskLst();
                            taskLst.TaskDescription = taskItems.TaskDescription;
                            taskLst.TaskWeight = taskItems.Weight;
                            taskLst.TRemark = taskItems.Remark;
                            taskLst.HasActParent = taskItems.HasActivityParent;

                            if (taskItems.HasActivityParent)
                            {

                                List<ActParentLst> actparentlsts = new List<ActParentLst>();
                                foreach (var actparent in taskItems.ActivitiesParents)
                                {
                                    ActParentLst actparentlst = new ActParentLst();
                                    actparentlst.ActParentDescription = actparent.ActivityParentDescription;
                                    actparentlst.ActParentWeight = actparent.Weight;
                                    actparentlst.ActpRemark = actparent.Remark;
                                    if (actparent.Activities != null)
                                    {

                                        List<ActivityLst> activityLsts = new List<ActivityLst>();
                                        foreach (var ActItems in actparent.Activities.Where(x => x.ActivityTargetDivisions != null))
                                        {
                                            ActivityLst lst = new ActivityLst();
                                            lst.ActivityDescription = ActItems.ActivityDescription;
                                            lst.Begining = ActItems.Begining;
                                            lst.MeasurementUnit = ActItems.UnitOfMeasurement.Name.ToString();
                                            lst.Target = ActItems.Goal;
                                            lst.Weight = ActItems.Weight;
                                            lst.Remark = ActItems.Remark;
                                            List<PlanOcc> planOccs = new List<PlanOcc>();

                                            var byQuarter = ActItems.ActivityTargetDivisions.OrderBy(x => x.Order).ToList();
                                            if (!QuarterMonth.Any())
                                            {

                                                int value = ReportBy == reporttype.Quarter.ToString() ? 4 : 12;
                                                if (ReportBy == reporttype.Quarter.ToString())
                                                {
                                                    for (int i = 0; i < 4; i++)
                                                    {
                                                        var quar = _dBContext.QuarterSettings.Single(x => x.QuarterOrder == i);


                                                        if (quar.StartMonth > 4)
                                                        {
                                                            quar.StartMonth = quar.StartMonth - 4;
                                                        }

                                                        else
                                                        {
                                                            quar.StartMonth = quar.StartMonth + 8;
                                                        }


                                                        if (quar.EndMonth > 4)
                                                        {
                                                            quar.EndMonth = quar.EndMonth - 4;
                                                        }

                                                        else
                                                        {
                                                            quar.EndMonth = quar.EndMonth + 8;
                                                        }



                                                        System.Globalization.DateTimeFormatInfo mfi = new
                                                            System.Globalization.DateTimeFormatInfo();
                                                        string fromG = mfi.GetMonthName(quar.StartMonth).ToString();

                                                        string toG = mfi.GetMonthName(quar.EndMonth).ToString();




                                                        QuarterMonth quarterMonths = new QuarterMonth();
                                                        quarterMonths.MonthName = "Quarter" + (i + 1);
                                                        QuarterMonth.Add(quarterMonths);
                                                    }
                                                    plannedReport.pMINT = 4;
                                                }
                                                else
                                                {
                                                    for (int i = 1; i <= 12; i++)
                                                    {
                                                        int k = 0;
                                                        if (i >= 7)
                                                        {
                                                            k = i - 6;
                                                        }

                                                        else
                                                        {
                                                            k = i + 6;
                                                        }

                                                        System.Globalization.DateTimeFormatInfo mfi = new
                                                            System.Globalization.DateTimeFormatInfo();
                                                        string strMonthName = mfi.GetMonthName(k).ToString();
                                                        QuarterMonth quarterMonths = new QuarterMonth();
                                                        quarterMonths.MonthName = strMonthName;
                                                        QuarterMonth.Add(quarterMonths);
                                                    }
                                                    plannedReport.pMINT = 12;
                                                }

                                                plannedReport.planDuration = QuarterMonth;
                                            }

                                            if (ReportBy == reporttype.Quarter.ToString())
                                            {

                                                for (int i = 0; i < 12; i += 3)
                                                {

                                                    PlanOcc planO = new PlanOcc();
                                                    planO.PlanTarget = byQuarter[i].Target + byQuarter[i + 1].Target + byQuarter[i + 2].Target;
                                                    planOccs.Add(planO);
                                                }


                                            }
                                            else
                                            {
                                                foreach (var itemQ in byQuarter)
                                                {
                                                    var progresslist = ActItems.ActProgress.Where(x => x.QuarterId == itemQ.Id).ToList();
                                                    PlanOcc planO = new PlanOcc();
                                                    planO.PlanTarget = itemQ.Target;
                                                    planOccs.Add(planO);
                                                }
                                            }
                                            lst.Plans = planOccs;
                                            activityLsts.Add(lst);
                                        }
                                        actparentlst.activityLsts = activityLsts;
                                        actparentlsts.Add(actparentlst);
                                    }
                                    else if (actparent.Activities.Any() && actparent.Activities.FirstOrDefault().targetDivision != null)
                                    {
                                        List<PlanOcc> planoccPlan = new List<PlanOcc>();
                                        var Pocu = actparent.Activities.FirstOrDefault();
                                        if (Pocu != null)
                                        {
                                            actparentlst.Target = Pocu.Goal;
                                            actparentlst.ActualWorked = Pocu.ActualWorked;
                                            actparentlst.MeasurementUnit = Pocu.UnitOfMeasurement.Name;
                                            actparentlst.Begining = Pocu.Begining;
                                            var byQuarter = Pocu.ActivityTargetDivisions.OrderBy(x => x.Order).ToList();
                                            if (!QuarterMonth.Any())
                                            {

                                                int value = ReportBy == reporttype.Quarter.ToString() ? 4 : 12;
                                                if (ReportBy == reporttype.Quarter.ToString())
                                                {
                                                    for (int i = 0; i < 4; i++)
                                                    {
                                                        var quar = _dBContext.QuarterSettings.Single(x => x.QuarterOrder == i);


                                                        if (quar.StartMonth > 4)
                                                        {
                                                            quar.StartMonth = quar.StartMonth - 4;
                                                        }

                                                        else
                                                        {
                                                            quar.StartMonth = quar.StartMonth + 8;
                                                        }


                                                        if (quar.EndMonth > 4)
                                                        {
                                                            quar.EndMonth = quar.EndMonth - 4;
                                                        }

                                                        else
                                                        {
                                                            quar.EndMonth = quar.EndMonth + 8;
                                                        }



                                                        System.Globalization.DateTimeFormatInfo mfi = new
                                                            System.Globalization.DateTimeFormatInfo();
                                                        string fromG = mfi.GetMonthName(quar.StartMonth).ToString();

                                                        string toG = mfi.GetMonthName(quar.EndMonth).ToString();




                                                        QuarterMonth quarterMonths = new QuarterMonth();
                                                        quarterMonths.MonthName = "Quarter" + (i + 1);
                                                        QuarterMonth.Add(quarterMonths);
                                                    }
                                                    plannedReport.pMINT = 4;
                                                }
                                                else
                                                {
                                                    for (int i = 1; i <= 12; i++)
                                                    {
                                                        int k = 0;
                                                        if (i >= 7)
                                                        {
                                                            k = i - 6;
                                                        }

                                                        else
                                                        {
                                                            k = i + 6;
                                                        }

                                                        System.Globalization.DateTimeFormatInfo mfi = new
                                                            System.Globalization.DateTimeFormatInfo();
                                                        string strMonthName = mfi.GetMonthName(k).ToString();
                                                        //int fromG = EthiopicDateTime.GetGregorianMonth(9, k, 1984);
                                                        //DateTime date = new DateTime(1984, fromG, 9);
                                                        QuarterMonth quarterMonths = new QuarterMonth();
                                                        quarterMonths.MonthName = strMonthName;
                                                        QuarterMonth.Add(quarterMonths);
                                                    }
                                                    plannedReport.pMINT = 12;
                                                }
                                                plannedReport.planDuration = QuarterMonth;
                                            }
                                            if (ReportBy == reporttype.Quarter.ToString())
                                            {

                                                for (int i = 0; i < 12; i += 3)
                                                {

                                                    PlanOcc planO = new PlanOcc();
                                                    planO.PlanTarget = byQuarter[i].Target + byQuarter[i + 1].Target + byQuarter[i + 2].Target;
                                                    planoccPlan.Add(planO);
                                                }
                                            }
                                            else
                                            {
                                                foreach (var itemQ in byQuarter)
                                                {
                                                    var progresslist = Pocu.ActProgress.Where(x => x.QuarterId == itemQ.Id).ToList();
                                                    PlanOcc planO = new PlanOcc();
                                                    planO.PlanTarget = itemQ.Target;
                                                    planoccPlan.Add(planO);
                                                }
                                            }
                                            actparentlst.ActDivisions = planoccPlan;

                                        }

                                        // actparentlst.Add(plns);

                                        actparentlsts.Add(actparentlst);
                                    }

                                }
                                taskLst.ActParentLst = actparentlsts;
                                taskLsts.Add(taskLst);

                            }
                            else if (taskItems.Activities.Any() && taskItems.Activities.FirstOrDefault().ActivityTargetDivisions != null)
                            {

                                var Acti = taskItems.Activities.FirstOrDefault();
                                if (Acti != null)
                                {
                                    taskLst.Begining = Acti.Begining;
                                    taskLst.MeasurementUnit = Acti.UnitOfMeasurement.Name;
                                    taskLst.Target = Acti.Goal;
                                    List<PlanOcc> planOccs = new List<PlanOcc>();
                                    var byQuarter = Acti.ActivityTargetDivisions.OrderBy(x => x.Order).ToList();
                                    if (!QuarterMonth.Any())
                                    {

                                        int value = ReportBy == reporttype.Quarter.ToString() ? 4 : 12;
                                        if (ReportBy == reporttype.Quarter.ToString())
                                        {
                                            for (int i = 0; i < 4; i++)
                                            {
                                                var quar = _dBContext.QuarterSettings.Single(x => x.QuarterOrder == i);

                                                if (quar.StartMonth > 4)
                                                {
                                                    quar.StartMonth = quar.StartMonth - 4;
                                                }

                                                else
                                                {
                                                    quar.StartMonth = quar.StartMonth + 8;
                                                }


                                                if (quar.EndMonth > 4)
                                                {
                                                    quar.EndMonth = quar.EndMonth - 4;
                                                }

                                                else
                                                {
                                                    quar.EndMonth = quar.EndMonth + 8;
                                                }



                                                System.Globalization.DateTimeFormatInfo mfi = new
                                                    System.Globalization.DateTimeFormatInfo();
                                                string fromG = mfi.GetMonthName(quar.StartMonth).ToString();

                                                string toG = mfi.GetMonthName(quar.EndMonth).ToString();




                                                QuarterMonth quarterMonths = new QuarterMonth();
                                                quarterMonths.MonthName = "Quarter" + (i + 1);
                                                QuarterMonth.Add(quarterMonths);
                                            }
                                            plannedReport.pMINT = 4;
                                        }
                                        else
                                        {
                                            for (int i = 1; i <= 12; i++)
                                            {
                                                int k = 0;
                                                if (i >= 7)
                                                {
                                                    k = i - 6;
                                                }

                                                else
                                                {
                                                    k = i + 6;
                                                }

                                                System.Globalization.DateTimeFormatInfo mfi = new
                                                    System.Globalization.DateTimeFormatInfo();
                                                string strMonthName = mfi.GetMonthName(k).ToString();
                                                //int fromG = EthiopicDateTime.GetGregorianMonth(9, k, 1984);
                                                //DateTime date = new DateTime(1984, fromG, 9);
                                                QuarterMonth quarterMonths = new QuarterMonth();
                                                quarterMonths.MonthName = strMonthName;
                                                QuarterMonth.Add(quarterMonths);
                                            }
                                            plannedReport.pMINT = 12;
                                        }
                                        plannedReport.planDuration = QuarterMonth;
                                    }
                                    if (ReportBy == reporttype.Quarter.ToString())
                                    {

                                        for (int i = 0; i < 12; i += 3)
                                        {

                                            PlanOcc planO = new PlanOcc();
                                            planO.PlanTarget = byQuarter[i].Target + byQuarter[i + 1].Target + byQuarter[i + 2].Target;
                                            planOccs.Add(planO);
                                        }


                                    }
                                    else
                                    {
                                        foreach (var itemQ in byQuarter)
                                        {
                                            var progresslist = Acti.ActProgress.Where(x => x.QuarterId == itemQ.Id).ToList();
                                            PlanOcc planO = new PlanOcc();
                                            planO.PlanTarget = itemQ.Target;
                                            planOccs.Add(planO);
                                        }
                                    }
                                    taskLst.TaskDivisions = planOccs;
                                    taskLsts.Add(taskLst);
                                }
                            }
                        }
                        plns.taskLsts = taskLsts;
                        plansLsts.Add(plns);
                    }
                    else if (plansItems.Activities.Any() && plansItems.Activities.FirstOrDefault().ActivityTargetDivisions != null)
                    {

                        List<PlanOcc> planoccPlan = new List<PlanOcc>();
                        var Pocu = plansItems.Activities.FirstOrDefault();
                        if (Pocu != null)
                        {
                            plns.Target = Pocu.Goal;
                            plns.ActualWorked = Pocu.ActualWorked;
                            plns.MeasurementUnit = Pocu.UnitOfMeasurement.Name;
                            plns.Begining = Pocu.Begining;
                            var byQuarter = Pocu.ActivityTargetDivisions.OrderBy(x => x.Order).ToList();
                            if (!QuarterMonth.Any())
                            {

                                int value = ReportBy == reporttype.Quarter.ToString() ? 4 : 12;
                                if (ReportBy == reporttype.Quarter.ToString())
                                {
                                    for (int i = 0; i < 4; i++)
                                    {
                                        var quar = _dBContext.QuarterSettings.Single(x => x.QuarterOrder == i);
                                        //DateTime fromG = Convert.ToDateTime(EthiopicDateTime.GetGregorianDate(9, quar.StartMonth, 1984));
                                        //DateTime toG = Convert.ToDateTime(EthiopicDateTime.GetGregorianDate(9, quar.EndMonth, 1984));


                                        if (quar.StartMonth > 4)
                                        {
                                            quar.StartMonth = quar.StartMonth - 4;
                                        }

                                        else
                                        {
                                            quar.StartMonth = quar.StartMonth + 8;
                                        }


                                        if (quar.EndMonth > 4)
                                        {
                                            quar.EndMonth = quar.EndMonth - 4;
                                        }

                                        else
                                        {
                                            quar.EndMonth = quar.EndMonth + 8;
                                        }



                                        System.Globalization.DateTimeFormatInfo mfi = new
                                            System.Globalization.DateTimeFormatInfo();
                                        string fromG = mfi.GetMonthName(quar.StartMonth).ToString();

                                        string toG = mfi.GetMonthName(quar.EndMonth).ToString();




                                        QuarterMonth quarterMonths = new QuarterMonth();
                                        quarterMonths.MonthName = "Quarter " + (i + 1);
                                        QuarterMonth.Add(quarterMonths);
                                    }
                                    plannedReport.pMINT = 4;
                                }
                                else
                                {
                                    for (int i = 1; i <= 12; i++)
                                    {
                                        int k = 0;
                                        if (i >= 7)
                                        {
                                            k = i - 6;
                                        }

                                        else
                                        {
                                            k = i + 6;
                                        }

                                        System.Globalization.DateTimeFormatInfo mfi = new
                                            System.Globalization.DateTimeFormatInfo();
                                        string strMonthName = mfi.GetMonthName(k).ToString();
                                        //int fromG = EthiopicDateTime.GetGregorianMonth(9, k, 1984);
                                        //DateTime date = new DateTime(1984, fromG, 9);
                                        QuarterMonth quarterMonths = new QuarterMonth();
                                        quarterMonths.MonthName = strMonthName;
                                        QuarterMonth.Add(quarterMonths);
                                    }
                                    plannedReport.pMINT = 12;
                                }
                                plannedReport.planDuration = QuarterMonth;
                            }
                            if (ReportBy == reporttype.Quarter.ToString())
                            {

                                for (int i = 0; i < 12; i += 3)
                                {

                                    PlanOcc planO = new PlanOcc();
                                    planO.PlanTarget = byQuarter[i].Target + byQuarter[i + 1].Target + byQuarter[i + 2].Target;
                                    planoccPlan.Add(planO);
                                }
                            }
                            else
                            {
                                foreach (var itemQ in byQuarter)
                                {
                                    var progresslist = Pocu.ActProgress.Where(x => x.QuarterId == itemQ.Id).ToList();
                                    PlanOcc planO = new PlanOcc();
                                    planO.PlanTarget = itemQ.Target;
                                    planoccPlan.Add(planO);
                                }
                            }
                            plns.PlanDivision = planoccPlan;

                        }

                        plansLsts.Add(plns);
                    }
                }

                plannedReport.PlansLst = plansLsts;

                return plannedReport;
            }
            catch (Exception e)
            {
                return plannedReport;
            }

        }


        public async Task<ProgresseReport> ProgresssReport(FilterationCriteria filterationCriteria)
        {

            // ViewBag.programs = Db.Programs.ToList();

            ProgresseReport progressReport = new ProgresseReport();

            progressReport.AllActivities = new List<ProgressReportTable>();

            try
            {
                string ReportType = "";
                string DateType = "";


                if (filterationCriteria.actParentId != Guid.Empty)
                {
                    List<QuarterMonth> QuarterMonth = new List<QuarterMonth>();
                    var BudgetYear = _dBContext.BudgetYears.Single(x => x.Year == filterationCriteria.budgetYear);
                    ReportType = "Activity Report for";
                    var allActivities = _dBContext.Activities

                        .Include(x => x.ActivityTargetDivisions)
                        .Where(x => x.ActivityParentId == filterationCriteria.actParentId && x.ActivityTargetDivisions != null).OrderBy(c => c.ShouldStat).Include(a => a.AssignedEmploye).Include(a => a.Task).ToList();
                    if (filterationCriteria.budgetYear != null)
                    {
                        DateType = "Year of " + " " + filterationCriteria.budgetYear;
                        allActivities = allActivities.Where(x => x.ShouldStat >= BudgetYear.FromDate && x.ShouldEnd <= BudgetYear.ToDate && x.Goal != 0).ToList();
                    }

                    foreach (var items in allActivities)
                    {

                        progressReport.planDuration = filterationCriteria.reporttype == reporttype.Quarter.ToString() ? 4 : 12;

                        //var  byQuarter = items.targetdivison.ToList().OrderBy(x => x.order);
                        if (!QuarterMonth.Any())
                        {

                            int value = items.targetDivision == TargetDivision.Quarterly ? 4 : 12;
                            if (filterationCriteria.reporttype == reporttype.Quarter.ToString())
                            {
                                for (int i = 0; i < 4; i++)
                                {
                                    var quar = _dBContext.QuarterSettings.Single(x => x.QuarterOrder == i);
                                    //DateTime fromG = Convert.ToDateTime(EthiopicDateTime.GetGregorianDate(9, quar.StartMonth, 1984));
                                    //DateTime toG = Convert.ToDateTime(EthiopicDateTime.GetGregorianDate(9, quar.EndMonth, 1984));


                                    if (quar.StartMonth > 4)
                                    {
                                        quar.StartMonth = quar.StartMonth - 4;
                                    }

                                    else
                                    {
                                        quar.StartMonth = quar.StartMonth + 8;
                                    }


                                    if (quar.EndMonth > 4)
                                    {
                                        quar.EndMonth = quar.EndMonth - 4;
                                    }

                                    else
                                    {
                                        quar.EndMonth = quar.EndMonth + 8;
                                    }



                                    System.Globalization.DateTimeFormatInfo mfi = new
                                        System.Globalization.DateTimeFormatInfo();
                                    string fromG = mfi.GetMonthName(quar.StartMonth).ToString();

                                    string toG = mfi.GetMonthName(quar.EndMonth).ToString();




                                    QuarterMonth quarterMonths = new QuarterMonth();
                                    quarterMonths.MonthName = "Quarter" + (i + 1);
                                    QuarterMonth.Add(quarterMonths);
                                }
                                // ViewBag.pMINT = 4;
                            }
                            else
                            {
                                for (int i = 1; i <= 12; i++)
                                {
                                    int h = 0;
                                    if (i >= 7)
                                    {
                                        h = i - 6;
                                    }

                                    else
                                    {
                                        h = i + 6;
                                    }

                                    System.Globalization.DateTimeFormatInfo mfi = new
                                        System.Globalization.DateTimeFormatInfo();
                                    string strMonthName = mfi.GetMonthName(h).ToString();
                                    //int fromG = EthiopicDateTime.GetGregorianMonth(9, k, 1984);
                                    //DateTime date = new DateTime(1984, fromG, 9);
                                    QuarterMonth quarterMonths = new QuarterMonth();
                                    quarterMonths.MonthName = strMonthName;
                                    QuarterMonth.Add(quarterMonths);
                                }
                                // ViewBag.pMINT = 12;
                            }
                            progressReport.planDuration2 = QuarterMonth;
                        }







                        ProgressReportTable Progress = new ProgressReportTable();
                        Progress.ActivityId = items.Id;
                        Progress.ActivityDescription = items.ActivityDescription;
                        Progress.StartDate = items.ShouldStat;
                        Progress.EndDate = items.ShouldEnd;
                        Progress.ActualStartDate = items.ActualStart == null ? items.ActualStart : items.ActualStart.Value;
                        Progress.ActualEndDate = items.ActualEnd == null ? items.ActualEnd : items.ActualEnd.Value;
                        Progress.PlanStartDate = EthiopicDateTime.GetEthiopicDate(items.ShouldStat.Day, items.ShouldStat.Month, items.ShouldStat.Year);
                        Progress.PlanEndDate = EthiopicDateTime.GetEthiopicDate(items.ShouldEnd.Day, items.ShouldEnd.Month, items.ShouldEnd.Year);
                        Progress.ProgressStartDate = items.ActualStart == null ? "" : EthiopicDateTime.GetEthiopicDate(items.ActualStart.Value.Day, items.ActualStart.Value.Month, items.ActualStart.Value.Year);
                        Progress.ProgressEndDate = items.ActualEnd == null ? "" : EthiopicDateTime.GetEthiopicDate(items.ActualEnd.Value.Day, items.ActualEnd.Value.Month, items.ActualEnd.Value.Year);
                        Progress.UsedBudget = items.ActualBudget;
                        Progress.PlannedBudget = items.PlanedBudget;
                        Progress.Status = items.Status;
                        var byQuarter = _dBContext.ActivityTargetDivisions.Where(x => x.ActivityId == items.Id).ToList().OrderBy(x => x.Order);
                        List<planOccurence> planOccurences = new List<planOccurence>();

                        foreach (var itemQ in byQuarter)
                        {
                            var progresslist = _dBContext.ActivityProgresses.Where(x => x.ActivityId == items.Id && x.QuarterId == itemQ.Id).ToList();
                            planOccurence planO = new planOccurence();
                            planO.Planned = itemQ.Target;
                            planO.Achivement = progresslist.Sum(x => x.ActualWorked);
                            planO.APercentile = planO.Planned == 0 ? 0 : (planO.Achivement / planO.Planned) * 100;
                            planOccurences.Add(planO);
                        }
                        List<planOccurence> planOccurencesq = new List<planOccurence>();

                        if (filterationCriteria.reporttype == reporttype.Quarter.ToString())
                        {

                            for (int i = 0; i < 12; i += 3)
                            {

                                planOccurence planOq = new planOccurence();


                                planOq.Planned = planOccurences[i].Planned + planOccurences[i + 1].Planned + planOccurences[i + 2].Planned;
                                planOq.Achivement = planOccurences[i].Achivement + planOccurences[i + 1].Achivement + planOccurences[i + 2].Achivement;
                                planOq.APercentile = planOq.Planned == 0 ? 0 : (planOq.Achivement / planOq.Planned) * 100; ;

                                planOccurencesq.Add(planOq);
                            }
                        }







                        Progress.planOccurences = (filterationCriteria.reporttype == reporttype.Quarter.ToString() ? planOccurencesq : planOccurences);
                        var employees = _dBContext.EmployeesAssignedForActivities.Where(x => x.ActivityId == items.Id).Select(z => z.EmployeeId);
                        var commites = new List<Guid>();
                        if (items.CommiteeId != null)
                        {
                            commites = _dBContext.Commitees.Single(x => x.Id == items.CommiteeId).Employees.Select(x => x.EmployeeId).ToList();
                        }

                        List<Employee> emp = new List<Employee>();
                        if (items.CommiteeId != null)
                        {
                            emp = _dBContext.Employees.Where(x => commites.Contains(x.Id)).ToList();
                        }
                        else
                        {
                            emp = _dBContext.Employees.Where(x => employees.Contains(x.Id)).ToList();
                        }
                        Progress.Employees = emp;
                        Progress.Begining = items.Begining;
                        Progress.ActualWorked = items.ActualWorked;
                        Progress.Weight = items.Weight;
                        Progress.Goal = items.Goal;
                        if (Progress.ActualWorked > 0)
                        {
                            if (Progress.ActualWorked == Progress.Goal)
                            {
                                Progress.Progress = 100;
                            }
                            else
                            {
                                float Nominator = Progress.ActualWorked;
                                float Denominator = (float)Progress.Goal;
                                Progress.Progress = (Nominator / Denominator) * 100;
                            }
                        }
                        else Progress.Progress = 0;

                        if (Progress.Progress > 0)
                        {
                            progressReport.AllActivities.Add(Progress);
                        }
                    }
                }





                if (filterationCriteria.taskId != Guid.Empty)
                {
                    List<QuarterMonth> QuarterMonth = new List<QuarterMonth>();
                    var BudgetYear = _dBContext.BudgetYears.Single(x => x.Year == filterationCriteria.budgetYear);
                    ReportType = "Activity Report for";

                    List<PM_Case_Managemnt_Infrustructure.Models.PM.Activity> actes = new List<PM_Case_Managemnt_Infrustructure.Models.PM.Activity>();
                    var task = _dBContext.Tasks
                        .Include(x => x.ActivitiesParents).ThenInclude(x => x.Activities).ThenInclude(x => x.ActivityTargetDivisions)
                        .Include(x => x.Activities).ThenInclude(x => x.ActivityTargetDivisions)
                        .Where(x => x.Id == filterationCriteria.taskId).FirstOrDefault();

                    if (task.HasActivityParent)
                    {

                        foreach (var activityparent in task.ActivitiesParents)
                        {

                            if (activityparent.HasActivity)
                            {
                                foreach (var act in activityparent.Activities)
                                {
                                    actes.Add(act);
                                }
                            }
                            else if (activityparent.Activities.Any() && activityparent.Activities.FirstOrDefault().targetDivision != null)
                            {
                                actes.Add(activityparent.Activities.FirstOrDefault());

                            }

                        }



                    }
                    else if (task.Activities.Any() && task.Activities.FirstOrDefault().ActivityTargetDivisions.Any())
                    {

                        actes.Add(task.Activities.FirstOrDefault());


                    }






                    var allActivities = actes.Where(x => x.ActivityTargetDivisions.Any()).OrderBy(c => c.ShouldStat).ToList();
                    if (filterationCriteria.budgetYear != null)
                    {
                        DateType = "Year of " + " " + filterationCriteria.budgetYear;
                        allActivities = allActivities.Where(x => x.ShouldStat >= BudgetYear.FromDate && x.ShouldEnd <= BudgetYear.ToDate && x.Goal != 0).ToList();
                    }

                    foreach (var items in allActivities)
                    {

                        progressReport.planDuration = filterationCriteria.reporttype == reporttype.Quarter.ToString() ? 4 : 12;


                        //var  byQuarter = items.targetdivison.ToList().OrderBy(x => x.order);
                        if (!QuarterMonth.Any())
                        {

                            int value = items.targetDivision == TargetDivision.Quarterly ? 4 : 12;
                            if (filterationCriteria.reporttype == reporttype.Quarter.ToString())
                            {
                                for (int i = 0; i < 4; i++)
                                {
                                    var quar = _dBContext.QuarterSettings.Single(x => x.QuarterOrder == i);
                                    //DateTime fromG = Convert.ToDateTime(EthiopicDateTime.GetGregorianDate(9, quar.StartMonth, 1984));
                                    //DateTime toG = Convert.ToDateTime(EthiopicDateTime.GetGregorianDate(9, quar.EndMonth, 1984));


                                    if (quar.StartMonth > 4)
                                    {
                                        quar.StartMonth = quar.StartMonth - 4;
                                    }

                                    else
                                    {
                                        quar.StartMonth = quar.StartMonth + 8;
                                    }


                                    if (quar.EndMonth > 4)
                                    {
                                        quar.EndMonth = quar.EndMonth - 4;
                                    }

                                    else
                                    {
                                        quar.EndMonth = quar.EndMonth + 8;
                                    }



                                    System.Globalization.DateTimeFormatInfo mfi = new
                                        System.Globalization.DateTimeFormatInfo();
                                    string fromG = mfi.GetMonthName(quar.StartMonth).ToString();

                                    string toG = mfi.GetMonthName(quar.EndMonth).ToString();




                                    QuarterMonth quarterMonths = new QuarterMonth();
                                    quarterMonths.MonthName = "Quarter" + (i + 1);
                                    QuarterMonth.Add(quarterMonths);
                                }
                                //ViewBag.pMINT = 4;
                            }
                            else
                            {
                                for (int i = 1; i <= 12; i++)
                                {
                                    int h = 0;
                                    if (i >= 7)
                                    {
                                        h = i - 6;
                                    }

                                    else
                                    {
                                        h = i + 6;
                                    }

                                    System.Globalization.DateTimeFormatInfo mfi = new
                                        System.Globalization.DateTimeFormatInfo();
                                    string strMonthName = mfi.GetMonthName(h).ToString();
                                    //int fromG = EthiopicDateTime.GetGregorianMonth(9, k, 1984);
                                    //DateTime date = new DateTime(1984, fromG, 9);
                                    QuarterMonth quarterMonths = new QuarterMonth();
                                    quarterMonths.MonthName = strMonthName;
                                    QuarterMonth.Add(quarterMonths);
                                }
                                //ViewBag.pMINT = 12;
                            }
                            progressReport.planDuration2 = QuarterMonth;
                        }







                        ProgressReportTable Progress = new ProgressReportTable();
                        Progress.ActivityId = items.Id;
                        Progress.ActivityDescription = items.ActivityDescription;
                        Progress.StartDate = items.ShouldStat;
                        Progress.EndDate = items.ShouldEnd;
                        Progress.ActualStartDate = items.ActualStart == null ? items.ActualStart : items.ActualStart.Value;
                        Progress.ActualEndDate = items.ActualEnd == null ? items.ActualEnd : items.ActualEnd.Value;
                        Progress.PlanStartDate = EthiopicDateTime.GetEthiopicDate(items.ShouldStat.Day, items.ShouldStat.Month, items.ShouldStat.Year);
                        Progress.PlanEndDate = EthiopicDateTime.GetEthiopicDate(items.ShouldEnd.Day, items.ShouldEnd.Month, items.ShouldEnd.Year);
                        Progress.ProgressStartDate = items.ActualStart == null ? "" : EthiopicDateTime.GetEthiopicDate(items.ActualStart.Value.Day, items.ActualStart.Value.Month, items.ActualStart.Value.Year);
                        Progress.ProgressEndDate = items.ActualEnd == null ? "" : EthiopicDateTime.GetEthiopicDate(items.ActualEnd.Value.Day, items.ActualEnd.Value.Month, items.ActualEnd.Value.Year);
                        Progress.UsedBudget = items.ActualBudget;
                        Progress.PlannedBudget = items.PlanedBudget;
                        Progress.Status = items.Status;
                        var byQuarter = _dBContext.ActivityTargetDivisions.Where(x => x.ActivityId == items.Id).ToList().OrderBy(x => x.Order);
                        List<planOccurence> planOccurences = new List<planOccurence>();

                        foreach (var itemQ in byQuarter)
                        {
                            var progresslist = _dBContext.ActivityProgresses.Where(x => x.ActivityId == items.Id && x.QuarterId == itemQ.Id).ToList();
                            planOccurence planO = new planOccurence();
                            planO.Planned = itemQ.Target;
                            planO.Achivement = progresslist.Sum(x => x.ActualWorked);
                            planO.APercentile = planO.Planned == 0 ? 0 : (planO.Achivement / planO.Planned) * 100;
                            planOccurences.Add(planO);
                        }
                        List<planOccurence> planOccurencesq = new List<planOccurence>();

                        if (filterationCriteria.reporttype == reporttype.Quarter.ToString())
                        {

                            for (int i = 0; i < 12; i += 3)
                            {

                                planOccurence planOq = new planOccurence();


                                planOq.Planned = planOccurences[i].Planned + planOccurences[i + 1].Planned + planOccurences[i + 2].Planned;
                                planOq.Achivement = planOccurences[i].Achivement + planOccurences[i + 1].Achivement + planOccurences[i + 2].Achivement;
                                planOq.APercentile = planOq.Planned == 0 ? 0 : (planOq.Achivement / planOq.Planned) * 100; ;

                                planOccurencesq.Add(planOq);
                            }
                        }







                        Progress.planOccurences = (filterationCriteria.reporttype == reporttype.Quarter.ToString() ? planOccurencesq : planOccurences);
                        var employees = _dBContext.EmployeesAssignedForActivities.Where(x => x.ActivityId == items.Id).Select(z => z.EmployeeId);
                        var commites = new List<Guid>();
                        if (items.CommiteeId != null)
                        {
                            commites = _dBContext.Commitees.Single(x => x.Id == items.CommiteeId).Employees.Select(x => x.EmployeeId).ToList();
                        }

                        List<Employee> emp = new List<Employee>();
                        if (items.CommiteeId != null)
                        {
                            emp = _dBContext.Employees.Where(x => commites.Contains(x.Id)).ToList();
                        }
                        else
                        {
                            emp = _dBContext.Employees.Where(x => employees.Contains(x.Id)).ToList();
                        }
                        Progress.Employees = emp;
                        Progress.Begining = items.Begining;
                        Progress.ActualWorked = items.ActualWorked;
                        Progress.Weight = items.Weight;
                        Progress.Goal = items.Goal;
                        if (Progress.ActualWorked > 0)
                        {
                            if (Progress.ActualWorked == Progress.Goal)
                            {
                                Progress.Progress = 100;
                            }
                            else
                            {
                                float Nominator = Progress.ActualWorked;
                                float Denominator = (float)Progress.Goal;
                                Progress.Progress = (Nominator / Denominator) * 100;
                            }
                        }
                        else Progress.Progress = 0;

                        if (Progress.Progress > 0)
                        {
                            progressReport.AllActivities.Add(Progress);
                        }



                    }
                    //ViewBag.selecttaskId = filterationCriteria.selecttaskId;
                }
                else if (filterationCriteria.planId != Guid.Empty)
                {
                    List<QuarterMonth> QuarterMonth = new List<QuarterMonth>();
                    var BudgetYear = _dBContext.BudgetYears.Single(x => x.Year == filterationCriteria.budgetYear);
                    ReportType = "Activity Report for";

                    List<PM_Case_Managemnt_Infrustructure.Models.PM.Activity> actes = new List<PM_Case_Managemnt_Infrustructure.Models.PM.Activity>();
                    var plan = _dBContext.Plans.Find(filterationCriteria.planId);

                    if (plan.HasTask)
                    {
                        foreach (var task in plan.Tasks)
                        {

                            if (task.HasActivityParent)
                            {

                                foreach (var activityparent in task.ActivitiesParents)
                                {

                                    if (activityparent.HasActivity)
                                    {
                                        foreach (var act in activityparent.Activities)
                                        {
                                            actes.Add(act);
                                        }
                                    }
                                    else if (activityparent.Activities.Any() && activityparent.Activities.FirstOrDefault().targetDivision != null)
                                    {
                                        actes.Add(activityparent.Activities.FirstOrDefault());

                                    }

                                }



                            }
                            else if (task.Activities.Any() && task.Activities.FirstOrDefault().targetDivision != null)
                            {

                                actes.Add(task.Activities.FirstOrDefault());


                            }
                        }
                    }
                    else if (plan.Activities.Any() && plan.Activities.FirstOrDefault().targetDivision != null)
                    {
                        actes.Add(plan.Activities.FirstOrDefault());

                    }






                    var allActivities = actes.Where(x => x.targetDivision != null).OrderBy(c => c.ShouldStat).ToList();
                    if (filterationCriteria.budgetYear != null)
                    {
                        DateType = "Year of " + " " + filterationCriteria.budgetYear;
                        allActivities = allActivities.Where(x => x.ShouldStat >= BudgetYear.FromDate && x.ShouldEnd <= BudgetYear.ToDate && x.Goal != 0).ToList();
                    }

                    foreach (var items in allActivities)
                    {

                        progressReport.planDuration = filterationCriteria.reporttype == reporttype.Quarter.ToString() ? 4 : 12;


                        //var  byQuarter = items.targetdivison.ToList().OrderBy(x => x.order);
                        if (!QuarterMonth.Any())
                        {

                            int value = items.targetDivision == TargetDivision.Quarterly ? 4 : 12;
                            if (filterationCriteria.reporttype == reporttype.Quarter.ToString())
                            {
                                for (int i = 0; i < 4; i++)
                                {
                                    var quar = _dBContext.QuarterSettings.Single(x => x.QuarterOrder == i);
                                    //DateTime fromG = Convert.ToDateTime(EthiopicDateTime.GetGregorianDate(9, quar.StartMonth, 1984));
                                    //DateTime toG = Convert.ToDateTime(EthiopicDateTime.GetGregorianDate(9, quar.EndMonth, 1984));


                                    if (quar.StartMonth > 4)
                                    {
                                        quar.StartMonth = quar.StartMonth - 4;
                                    }

                                    else
                                    {
                                        quar.StartMonth = quar.StartMonth + 8;
                                    }


                                    if (quar.EndMonth > 4)
                                    {
                                        quar.EndMonth = quar.EndMonth - 4;
                                    }

                                    else
                                    {
                                        quar.EndMonth = quar.EndMonth + 8;
                                    }



                                    System.Globalization.DateTimeFormatInfo mfi = new
                                        System.Globalization.DateTimeFormatInfo();
                                    string fromG = mfi.GetMonthName(quar.StartMonth).ToString();

                                    string toG = mfi.GetMonthName(quar.EndMonth).ToString();




                                    QuarterMonth quarterMonths = new QuarterMonth();
                                    quarterMonths.MonthName = "Quarter" + (i + 1);
                                    QuarterMonth.Add(quarterMonths);
                                }
                                //ViewBag.pMINT = 4;
                            }
                            else
                            {
                                for (int i = 1; i <= 12; i++)
                                {
                                    int h = 0;
                                    if (i >= 7)
                                    {
                                        h = i - 6;
                                    }

                                    else
                                    {
                                        h = i + 6;
                                    }

                                    System.Globalization.DateTimeFormatInfo mfi = new
                                        System.Globalization.DateTimeFormatInfo();
                                    string strMonthName = mfi.GetMonthName(h).ToString();
                                    //int fromG = EthiopicDateTime.GetGregorianMonth(9, k, 1984);
                                    //DateTime date = new DateTime(1984, fromG, 9);
                                    QuarterMonth quarterMonths = new QuarterMonth();
                                    quarterMonths.MonthName = strMonthName;
                                    QuarterMonth.Add(quarterMonths);
                                }
                                //ViewBag.pMINT = 12;
                            }
                            progressReport.planDuration2 = QuarterMonth;
                        }







                        ProgressReportTable Progress = new ProgressReportTable();
                        Progress.ActivityId = items.Id;
                        Progress.ActivityDescription = items.ActivityDescription;
                        Progress.StartDate = items.ShouldStat;
                        Progress.EndDate = items.ShouldEnd;
                        Progress.ActualStartDate = items.ActualStart == null ? items.ActualStart : items.ActualStart.Value;
                        Progress.ActualEndDate = items.ActualEnd == null ? items.ActualEnd : items.ActualEnd.Value;
                        Progress.PlanStartDate = EthiopicDateTime.GetEthiopicDate(items.ShouldStat.Day, items.ShouldStat.Month, items.ShouldStat.Year);
                        Progress.PlanEndDate = EthiopicDateTime.GetEthiopicDate(items.ShouldEnd.Day, items.ShouldEnd.Month, items.ShouldEnd.Year);
                        Progress.ProgressStartDate = items.ActualStart == null ? "" : EthiopicDateTime.GetEthiopicDate(items.ActualStart.Value.Day, items.ActualStart.Value.Month, items.ActualStart.Value.Year);
                        Progress.ProgressEndDate = items.ActualEnd == null ? "" : EthiopicDateTime.GetEthiopicDate(items.ActualEnd.Value.Day, items.ActualEnd.Value.Month, items.ActualEnd.Value.Year);
                        Progress.UsedBudget = items.ActualBudget;
                        Progress.PlannedBudget = items.PlanedBudget;
                        Progress.Status = items.Status;
                        var byQuarter = _dBContext.ActivityTargetDivisions.Where(x => x.ActivityId == items.Id).ToList().OrderBy(x => x.Order);
                        List<planOccurence> planOccurences = new List<planOccurence>();

                        foreach (var itemQ in byQuarter)
                        {
                            var progresslist = _dBContext.ActivityProgresses.Where(x => x.ActivityId == items.Id && x.QuarterId == itemQ.Id).ToList();
                            planOccurence planO = new planOccurence();
                            planO.Planned = itemQ.Target;
                            planO.Achivement = progresslist.Sum(x => x.ActualWorked);
                            planO.APercentile = planO.Planned == 0 ? 0 : (planO.Achivement / planO.Planned) * 100;
                            planOccurences.Add(planO);
                        }



                        List<planOccurence> planOccurencesq = new List<planOccurence>();

                        if (filterationCriteria.reporttype == reporttype.Quarter.ToString())
                        {

                            for (int i = 0; i < 12; i += 3)
                            {

                                planOccurence planOq = new planOccurence();


                                planOq.Planned = planOccurences[i].Planned + planOccurences[i + 1].Planned + planOccurences[i + 2].Planned;
                                planOq.Achivement = planOccurences[i].Achivement + planOccurences[i + 1].Achivement + planOccurences[i + 2].Achivement;
                                planOq.APercentile = planOq.Planned == 0 ? 0 : (planOq.Achivement / planOq.Planned) * 100; ;

                                planOccurencesq.Add(planOq);
                            }
                        }







                        Progress.planOccurences = (filterationCriteria.reporttype == reporttype.Quarter.ToString() ? planOccurencesq : planOccurences);
                        var employees = _dBContext.EmployeesAssignedForActivities.Where(x => x.ActivityId == items.Id).Select(z => z.EmployeeId);
                        var commites = new List<Guid>();
                        if (items.CommiteeId != null)
                        {
                            commites = _dBContext.Commitees.Single(x => x.Id == items.CommiteeId).Employees.Select(x => x.EmployeeId).ToList();
                        }

                        List<Employee> emp = new List<Employee>();
                        if (items.CommiteeId != null)
                        {
                            emp = _dBContext.Employees.Where(x => commites.Contains(x.Id)).ToList();
                        }
                        else
                        {
                            emp = _dBContext.Employees.Where(x => employees.Contains(x.Id)).ToList();
                        }
                        Progress.Employees = emp;
                        Progress.Begining = items.Begining;
                        Progress.ActualWorked = items.ActualWorked;
                        Progress.Weight = items.Weight;
                        Progress.Goal = items.Goal;
                        if (Progress.ActualWorked > 0)
                        {
                            if (Progress.ActualWorked == Progress.Goal)
                            {
                                Progress.Progress = 100;
                            }
                            else
                            {
                                float Nominator = Progress.ActualWorked;
                                float Denominator = Math.Abs((float)Progress.Goal - (float)Progress.Begining);
                                Progress.Progress = (Nominator / Denominator) * 100;
                            }
                        }
                        else Progress.Progress = 0;

                        if (Progress.Progress > 0)
                        {
                            progressReport.AllActivities.Add(Progress);
                        }
                    }
                }

                //ViewBag.reportMessage = ReportType + " the " + DateType;
                //ViewBag.AllActivities = ListsOfTable.OrderByDescending(x => x.Progress);
                //return View();
                progressReport.reportMessage = ReportType + " the " + DateType;
                progressReport.AllActivities.OrderByDescending(x => x.Progress).ToList();

                return progressReport;
            }
            catch (Exception e)
            {
                return progressReport;
            }


        }

        public async Task<ProgresseReportByStructure> GetProgressByStructure(int BudgetYea, Guid selectStructureId, string ReportBy)
        {
            ProgresseReportByStructure progresseReportByStructure = new ProgresseReportByStructure();
            progresseReportByStructure.plansLsts = new List<PlansLst>();

            try
            {
                if (selectStructureId != null)
                {
                    var allPlans = _dBContext.Plans.Include(x => x.Program)
                          .Include(x => x.Tasks).ThenInclude(x => x.ActivitiesParents).ThenInclude(x => x.Activities).ThenInclude(x => x.UnitOfMeasurement)
                                 .Include(x => x.Tasks).ThenInclude(x => x.Activities).ThenInclude(x => x.UnitOfMeasurement)
                                 .Include(x => x.Activities).ThenInclude(x => x.UnitOfMeasurement)
                                 .Include(x => x.Tasks).ThenInclude(x => x.ActivitiesParents).ThenInclude(x => x.Activities).ThenInclude(x => x.ActivityTargetDivisions)
                                 .Include(x => x.Tasks).ThenInclude(x => x.Activities).ThenInclude(x => x.ActivityTargetDivisions)
                                 .Include(x => x.Activities).ThenInclude(x => x.ActivityTargetDivisions)

                        .Where(x => x.StructureId == selectStructureId).OrderBy(c => c.PeriodStartAt).ToList();
                    var BudgetYear = _dBContext.BudgetYears.Single(x => x.Year == BudgetYea);
                    progresseReportByStructure.PreviousBudgetYear = (BudgetYear.Year - 1).ToString();
                    allPlans = allPlans.Where(x => x.BudgetYearId == BudgetYear.Id).ToList();

                    progresseReportByStructure.planDuration = 0;
                    List<QuarterMonth> QuarterMonth = new List<QuarterMonth>();



                    foreach (var plansItems in allPlans)
                    {
                        PlansLst plns = new PlansLst();
                        plns.PlanName = plansItems.PlanName;
                        plns.Weight = plansItems.PlanWeight;
                        plns.PlRemark = plansItems.Remark;
                        plns.HasTask = plansItems.HasTask;
                        if (plansItems.HasTask)
                        {
                            List<TaskLst> taskLsts = new List<TaskLst>();

                            foreach (var taskItems in plansItems.Tasks)
                            {
                                TaskLst taskLst = new TaskLst();
                                taskLst.TaskDescription = taskItems.TaskDescription;
                                taskLst.TaskWeight = taskItems.Weight;
                                taskLst.TRemark = taskItems.Remark;
                                taskLst.HasActParent = taskItems.HasActivityParent;

                                if (taskItems.HasActivityParent)
                                {
                                    List<ActParentLst> ActParentLst = new List<ActParentLst>();
                                    foreach (var actparentItems in taskItems.ActivitiesParents)
                                    {

                                        ActParentLst actparent = new ActParentLst();
                                        actparent.ActParentDescription = actparentItems.ActivityParentDescription;
                                        actparent.ActParentWeight = actparentItems.Weight;
                                        actparent.ActpRemark = actparentItems.Remark;
                                        if (actparentItems.HasActivity)
                                        {
                                            List<ActivityLst> activityLsts = new List<ActivityLst>();

                                            foreach (var ActItems in actparentItems.Activities.Where(x => x.targetDivision != null))
                                            {
                                                ActivityLst lst = new ActivityLst();
                                                lst.ActivityDescription = ActItems.ActivityDescription;
                                                lst.Begining = ActItems.Begining;
                                                lst.MeasurementUnit = ActItems.UnitOfMeasurement.Name;
                                                lst.Target = ActItems.Goal;
                                                lst.Weight = ActItems.Weight;
                                                lst.Remark = ActItems.Remark;
                                                lst.ActualWorked = (float)Math.Round(ActItems.ActualWorked);
                                                if (lst.ActualWorked > 0)
                                                {
                                                    if (lst.ActualWorked == lst.Target)
                                                    {
                                                        lst.Progress = 100;
                                                    }
                                                    else
                                                    {
                                                        float Nominator = lst.ActualWorked;
                                                        float Denominator = (float)lst.Target;
                                                        lst.Progress = (float)Math.Round((Nominator / Denominator) * 100, 2);
                                                    }
                                                }
                                                else lst.Progress = 0;
                                                List<PlanOcc> planOccs = new List<PlanOcc>();
                                                if (progresseReportByStructure.planDuration == 0)
                                                {
                                                    progresseReportByStructure.planDuration = ReportBy == reporttype.Quarter.ToString() ? 4 : 12;
                                                }
                                                var byQuarter = _dBContext.ActivityTargetDivisions.Where(x => x.ActivityId == ActItems.Id).OrderBy(x => x.Order).ToList();





                                                if (!QuarterMonth.Any())
                                                {

                                                    int value = ReportBy == reporttype.Quarter.ToString() ? 4 : 12;
                                                    if (ReportBy == reporttype.Quarter.ToString())
                                                    {
                                                        for (int i = 0; i < 4; i++)
                                                        {
                                                            var quar = _dBContext.QuarterSettings.Single(x => x.QuarterOrder == i);


                                                            if (quar.StartMonth > 4)
                                                            {
                                                                quar.StartMonth = quar.StartMonth - 4;
                                                            }

                                                            else
                                                            {
                                                                quar.StartMonth = quar.StartMonth + 8;
                                                            }


                                                            if (quar.EndMonth > 4)
                                                            {
                                                                quar.EndMonth = quar.EndMonth - 4;
                                                            }

                                                            else
                                                            {
                                                                quar.EndMonth = quar.EndMonth + 8;
                                                            }



                                                            System.Globalization.DateTimeFormatInfo mfi = new
                                                                System.Globalization.DateTimeFormatInfo();
                                                            string fromG = mfi.GetMonthName(quar.StartMonth).ToString();

                                                            string toG = mfi.GetMonthName(quar.EndMonth).ToString();




                                                            QuarterMonth quarterMonths = new QuarterMonth();
                                                            quarterMonths.MonthName = "Quarter" + (i + 1);
                                                            QuarterMonth.Add(quarterMonths);
                                                        }
                                                        progresseReportByStructure.pMINT = 4;
                                                    }
                                                    else
                                                    {
                                                        for (int i = 1; i <= 12; i++)
                                                        {
                                                            int k = 0;
                                                            if (i >= 7)
                                                            {
                                                                k = i - 6;
                                                            }

                                                            else
                                                            {
                                                                k = i + 6;
                                                            }

                                                            System.Globalization.DateTimeFormatInfo mfi = new
                                                                System.Globalization.DateTimeFormatInfo();
                                                            string strMonthName = mfi.GetMonthName(k).ToString();

                                                            QuarterMonth quarterMonths = new QuarterMonth();
                                                            quarterMonths.MonthName = strMonthName;
                                                            QuarterMonth.Add(quarterMonths);
                                                        }
                                                        progresseReportByStructure.pMINT = 12;
                                                    }
                                                    progresseReportByStructure.planDuration2 = QuarterMonth;
                                                }






                                                List<PlanOcc> planOq = new List<PlanOcc>();
                                                foreach (var itemQ in byQuarter)
                                                {
                                                    var progresslist = _dBContext.ActivityProgresses.Where(x => x.QuarterId == itemQ.Id && x.QuarterId == itemQ.Id).ToList();
                                                    PlanOcc planO = new PlanOcc();
                                                    planO.PlanTarget = itemQ.Target;
                                                    planO.ActualWorked = progresslist.Sum(x => x.ActualWorked);
                                                    planO.Percentile = planO.PlanTarget == 0 ? 0 : (float)Math.Round((planO.ActualWorked / planO.PlanTarget) * 100, 2);
                                                    planOq.Add(planO);
                                                }
                                                if (ReportBy == reporttype.Quarter.ToString())
                                                {



                                                    for (int i = 0; i < 12; i += 3)
                                                    {


                                                        PlanOcc planO = new PlanOcc();
                                                        planO.PlanTarget = planOq[i].PlanTarget + planOq[i + 1].PlanTarget + planOq[i + 2].PlanTarget;
                                                        planO.ActualWorked = planOq[i].ActualWorked + planOq[i + 1].ActualWorked + planOq[i + 2].ActualWorked;
                                                        planO.Percentile = planO.PlanTarget == 0 ? 0 : (float)Math.Round((planO.ActualWorked / planO.PlanTarget) * 100, 2);
                                                        planOccs.Add(planO);
                                                    }


                                                }
                                                else
                                                {
                                                    planOccs = planOq;

                                                }
                                                lst.Plans = planOccs;
                                                activityLsts.Add(lst);
                                            }
                                            actparent.activityLsts = activityLsts;
                                        }
                                        else if (actparentItems.Activities.Any() && actparentItems.Activities.FirstOrDefault().targetDivision != null)
                                        {
                                            var TaskOcs = actparentItems.Activities.FirstOrDefault();
                                            if (TaskOcs != null)
                                            {
                                                actparent.Begining = TaskOcs.Begining;
                                                actparent.MeasurementUnit = TaskOcs.UnitOfMeasurement.Name;
                                                actparent.Target = TaskOcs.Goal;
                                                actparent.ActParentWeight = TaskOcs.Weight;
                                                actparent.ActualWorked = (float)Math.Round(TaskOcs.ActualWorked, 2);
                                                if (actparent.ActualWorked > 0)
                                                {
                                                    if (actparent.ActualWorked == actparent.Target)
                                                    {
                                                        actparent.Progress = 100;
                                                    }
                                                    else
                                                    {
                                                        float Nominator = (float)actparent.ActualWorked;
                                                        float Denominator = (float)actparent.Target;
                                                        actparent.Progress = (float)Math.Round((Nominator / Denominator) * 100, 2);
                                                    }
                                                }
                                                else actparent.Progress = 0;
                                                List<PlanOcc> planOccs = new List<PlanOcc>();
                                                if (progresseReportByStructure.planDuration == 0)
                                                {
                                                    progresseReportByStructure.planDuration = ReportBy == reporttype.Quarter.ToString() ? 4 : 12;
                                                }
                                                var byQuarter = TaskOcs.ActivityTargetDivisions.ToList().OrderBy(x => x.Order);

                                                if (!QuarterMonth.Any())
                                                {
                                                    int value = ReportBy == reporttype.Quarter.ToString() ? 4 : 12;
                                                    if (ReportBy == reporttype.Quarter.ToString())
                                                    {
                                                        for (int i = 0; i < 4; i++)
                                                        {
                                                            var quar = _dBContext.QuarterSettings.Single(x => x.QuarterOrder == i);

                                                            if (quar.StartMonth > 4)
                                                            {
                                                                quar.StartMonth = quar.StartMonth - 4;
                                                            }
                                                            else
                                                            {
                                                                quar.StartMonth = quar.StartMonth + 8;
                                                            }

                                                            if (quar.EndMonth > 4)
                                                            {
                                                                quar.EndMonth = quar.EndMonth - 4;
                                                            }
                                                            else
                                                            {
                                                                quar.EndMonth = quar.EndMonth + 8;
                                                            }

                                                            System.Globalization.DateTimeFormatInfo mfi = new
                                                                System.Globalization.DateTimeFormatInfo();
                                                            string fromG = mfi.GetMonthName(quar.StartMonth).ToString();

                                                            string toG = mfi.GetMonthName(quar.EndMonth).ToString();

                                                            QuarterMonth quarterMonths = new QuarterMonth();
                                                            quarterMonths.MonthName = "Quarter" + (i + 1);
                                                            QuarterMonth.Add(quarterMonths);
                                                        }
                                                        progresseReportByStructure.pMINT = 4;
                                                    }
                                                    else
                                                    {
                                                        for (int i = 1; i <= 12; i++)
                                                        {
                                                            int k = 0;
                                                            if (i >= 7)
                                                            {
                                                                k = i - 6;
                                                            }

                                                            else
                                                            {
                                                                k = i + 6;
                                                            }

                                                            System.Globalization.DateTimeFormatInfo mfi = new
                                                                System.Globalization.DateTimeFormatInfo();
                                                            string strMonthName = mfi.GetMonthName(k).ToString();

                                                            QuarterMonth quarterMonths = new QuarterMonth();
                                                            quarterMonths.MonthName = strMonthName;
                                                            QuarterMonth.Add(quarterMonths);
                                                        }
                                                        progresseReportByStructure.pMINT = 12;
                                                    }
                                                    progresseReportByStructure.planDuration2 = QuarterMonth;
                                                }

                                                List<PlanOcc> planOq = new List<PlanOcc>();
                                                foreach (var itemQ in byQuarter)
                                                {
                                                    var progresslist = _dBContext.ActivityProgresses.Where(x => x.QuarterId == itemQ.Id && x.QuarterId == itemQ.Id).ToList();
                                                    PlanOcc planO = new PlanOcc();
                                                    planO.PlanTarget = itemQ.Target;
                                                    planO.ActualWorked = progresslist.Sum(x => x.ActualWorked);
                                                    planO.Percentile = planO.PlanTarget == 0 ? 0 : (float)Math.Round((planO.ActualWorked / planO.PlanTarget) * 100, 2);
                                                    planOq.Add(planO);
                                                }
                                                if (ReportBy == reporttype.Quarter.ToString())
                                                {

                                                    if (planOq.Any())
                                                    {
                                                        for (int i = 0; i < 12; i += 3)
                                                        {
                                                            PlanOcc planO = new PlanOcc();
                                                            planO.PlanTarget = planOq[i].PlanTarget + planOq[i + 1].PlanTarget + planOq[i + 2].PlanTarget;
                                                            planO.ActualWorked = planOq[i].ActualWorked + planOq[i + 1].ActualWorked + planOq[i + 2].ActualWorked;
                                                            planO.Percentile = planO.PlanTarget == 0 ? 0 : (float)Math.Round((planO.ActualWorked / planO.PlanTarget) * 100, 2);
                                                            planOccs.Add(planO);
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    planOccs = planOq;

                                                }
                                                actparent.ActDivisions = planOccs;
                                            }

                                        }
                                        ActParentLst.Add(actparent);
                                    }

                                    taskLst.ActParentLst = ActParentLst;
                                    taskLsts.Add(taskLst);
                                }
                                else if (taskItems.Activities.Any() && taskItems.Activities.FirstOrDefault().ActivityTargetDivisions.Any())
                                {
                                    var TaskOcs = taskItems.Activities.FirstOrDefault();
                                    if (TaskOcs != null)
                                    {
                                        taskLst.Begining = TaskOcs.Begining;
                                        taskLst.MeasurementUnit = TaskOcs.UnitOfMeasurement.Name;
                                        taskLst.Target = TaskOcs.Goal;
                                        taskLst.TaskWeight = TaskOcs.Weight;
                                        taskLst.ActualWorked = (float)Math.Round(TaskOcs.ActualWorked, 2);
                                        if (taskLst.ActualWorked > 0)
                                        {
                                            if (taskLst.ActualWorked == taskLst.Target)
                                            {
                                                taskLst.Progress = 100;
                                            }
                                            else
                                            {
                                                float Nominator = (float)taskLst.ActualWorked;
                                                float Denominator = (float)taskLst.Target;
                                                taskLst.Progress = (float)Math.Round((Nominator / Denominator) * 100, 2);
                                            }
                                        }
                                        else taskLst.Progress = 0;
                                        List<PlanOcc> planOccs = new List<PlanOcc>();
                                        if (progresseReportByStructure.planDuration == 0)
                                        {
                                            progresseReportByStructure.planDuration = ReportBy == reporttype.Quarter.ToString() ? 4 : 12;
                                        }
                                        var byQuarter = TaskOcs.ActivityTargetDivisions.ToList().OrderBy(x => x.Order);

                                        if (!QuarterMonth.Any())
                                        {
                                            int value = ReportBy == reporttype.Quarter.ToString() ? 4 : 12;
                                            if (ReportBy == reporttype.Quarter.ToString())
                                            {
                                                for (int i = 0; i < 4; i++)
                                                {
                                                    var quar = _dBContext.QuarterSettings.Single(x => x.QuarterOrder == i);

                                                    if (quar.StartMonth > 4)
                                                    {
                                                        quar.StartMonth = quar.StartMonth - 4;
                                                    }
                                                    else
                                                    {
                                                        quar.StartMonth = quar.StartMonth + 8;
                                                    }

                                                    if (quar.EndMonth > 4)
                                                    {
                                                        quar.EndMonth = quar.EndMonth - 4;
                                                    }
                                                    else
                                                    {
                                                        quar.EndMonth = quar.EndMonth + 8;
                                                    }

                                                    System.Globalization.DateTimeFormatInfo mfi = new
                                                        System.Globalization.DateTimeFormatInfo();
                                                    string fromG = mfi.GetMonthName(quar.StartMonth).ToString();

                                                    string toG = mfi.GetMonthName(quar.EndMonth).ToString();

                                                    QuarterMonth quarterMonths = new QuarterMonth();
                                                    quarterMonths.MonthName = "Quarter" + (i + 1);
                                                    QuarterMonth.Add(quarterMonths);
                                                }
                                                progresseReportByStructure.pMINT = 4;
                                            }
                                            else
                                            {
                                                for (int i = 1; i <= 12; i++)
                                                {
                                                    int k = 0;
                                                    if (i >= 7)
                                                    {
                                                        k = i - 6;
                                                    }

                                                    else
                                                    {
                                                        k = i + 6;
                                                    }

                                                    System.Globalization.DateTimeFormatInfo mfi = new
                                                        System.Globalization.DateTimeFormatInfo();
                                                    string strMonthName = mfi.GetMonthName(k).ToString();

                                                    QuarterMonth quarterMonths = new QuarterMonth();
                                                    quarterMonths.MonthName = strMonthName;
                                                    QuarterMonth.Add(quarterMonths);
                                                }
                                                progresseReportByStructure.pMINT = 12;
                                            }
                                            progresseReportByStructure.planDuration2 = QuarterMonth;
                                        }

                                        List<PlanOcc> planOq = new List<PlanOcc>();
                                        foreach (var itemQ in byQuarter)
                                        {
                                            var progresslist = _dBContext.ActivityProgresses.Where(x => x.QuarterId == itemQ.Id && x.QuarterId == itemQ.Id).ToList();
                                            PlanOcc planO = new PlanOcc();
                                            planO.PlanTarget = itemQ.Target;
                                            planO.ActualWorked = progresslist.Sum(x => x.ActualWorked);
                                            planO.Percentile = planO.PlanTarget == 0 ? 0 : (float)Math.Round((planO.ActualWorked / planO.PlanTarget) * 100, 2);
                                            planOq.Add(planO);
                                        }
                                        if (ReportBy == reporttype.Quarter.ToString())
                                        {

                                            for (int i = 0; i < 12; i += 3)
                                            {
                                                PlanOcc planO = new PlanOcc();
                                                planO.PlanTarget = planOq[i].PlanTarget + planOq[i + 1].PlanTarget + planOq[i + 2].PlanTarget;
                                                planO.ActualWorked = planOq[i].ActualWorked + planOq[i + 1].ActualWorked + planOq[i + 2].ActualWorked;
                                                planO.Percentile = planO.PlanTarget == 0 ? 0 : (float)Math.Round((planO.ActualWorked / planO.PlanTarget) * 100, 2);
                                                planOccs.Add(planO);
                                            }
                                        }
                                        else
                                        {
                                            planOccs = planOq;

                                        }
                                        taskLst.TaskDivisions = planOccs;
                                    }
                                    taskLsts.Add(taskLst);
                                }
                            }
                            plns.taskLsts = taskLsts;
                            progresseReportByStructure.plansLsts.Add(plns);
                        }
                        else if (plansItems.Activities.Any() && plansItems.Activities.FirstOrDefault().ActivityTargetDivisions.Any())
                        {
                            List<ActivityLst> activityLsts = new List<ActivityLst>();
                            var PlanDivOcs = plansItems.Activities.FirstOrDefault();
                            if (PlanDivOcs != null)
                            {
                                plns.Begining = PlanDivOcs.Begining;
                                plns.MeasurementUnit = PlanDivOcs.UnitOfMeasurement.Name;
                                plns.Target = PlanDivOcs.Goal;
                                plns.Weight = PlanDivOcs.Weight;
                                plns.ActualWorked = (float)Math.Round(PlanDivOcs.ActualWorked, 2);
                                if (plns.ActualWorked > 0)
                                {
                                    if (plns.ActualWorked == plns.Target)
                                    {
                                        plns.Progress = 100;
                                    }
                                    else
                                    {
                                        float Nominator = (float)plns.ActualWorked;
                                        float Denominator = (float)plns.Target;
                                        plns.Progress = (float)Math.Round((Nominator / Denominator) * 100, 2);
                                    }
                                }
                                else plns.Progress = 0;
                                List<PlanOcc> planOccs = new List<PlanOcc>();
                                if (progresseReportByStructure.planDuration == 0)
                                {
                                    progresseReportByStructure.planDuration = ReportBy == reporttype.Quarter.ToString() ? 4 : 12;
                                }
                                var byQuarter = PlanDivOcs.ActivityTargetDivisions.ToList().OrderBy(x => x.Order);
                                if (!QuarterMonth.Any())
                                {

                                    int value = ReportBy == reporttype.Quarter.ToString() ? 4 : 12;
                                    if (ReportBy == reporttype.Quarter.ToString())
                                    {
                                        for (int i = 0; i < 4; i++)
                                        {
                                            var quar = _dBContext.QuarterSettings.Single(x => x.QuarterOrder == i);



                                            if (quar.StartMonth > 4)
                                            {
                                                quar.StartMonth = quar.StartMonth - 4;
                                            }

                                            else
                                            {
                                                quar.StartMonth = quar.StartMonth + 8;
                                            }


                                            if (quar.EndMonth > 4)
                                            {
                                                quar.EndMonth = quar.EndMonth - 4;
                                            }

                                            else
                                            {
                                                quar.EndMonth = quar.EndMonth + 8;
                                            }



                                            System.Globalization.DateTimeFormatInfo mfi = new
                                                System.Globalization.DateTimeFormatInfo();
                                            string fromG = mfi.GetMonthName(quar.StartMonth).ToString();

                                            string toG = mfi.GetMonthName(quar.EndMonth).ToString();




                                            QuarterMonth quarterMonths = new QuarterMonth();
                                            quarterMonths.MonthName = "Quarter" + (i + 1);
                                            QuarterMonth.Add(quarterMonths);
                                        }
                                        progresseReportByStructure.pMINT = 4;
                                    }
                                    else
                                    {
                                        for (int i = 1; i <= 12; i++)
                                        {
                                            int k = 0;
                                            if (i >= 7)
                                            {
                                                k = i - 6;
                                            }

                                            else
                                            {
                                                k = i + 6;
                                            }

                                            System.Globalization.DateTimeFormatInfo mfi = new
                                                System.Globalization.DateTimeFormatInfo();
                                            string strMonthName = mfi.GetMonthName(k).ToString();

                                            QuarterMonth quarterMonths = new QuarterMonth();
                                            quarterMonths.MonthName = strMonthName;
                                            QuarterMonth.Add(quarterMonths);
                                        }
                                        progresseReportByStructure.pMINT = 12;
                                    }
                                    progresseReportByStructure.planDuration2 = QuarterMonth;
                                }


                                List<PlanOcc> planOq = new List<PlanOcc>();
                                foreach (var itemQ in byQuarter)
                                {
                                    var progresslist = _dBContext.ActivityProgresses.Where(x => x.QuarterId == itemQ.Id).ToList();
                                    PlanOcc planO = new PlanOcc();
                                    planO.PlanTarget = itemQ.Target;
                                    planO.ActualWorked = progresslist.Sum(x => x.ActualWorked);
                                    planO.Percentile = planO.PlanTarget == 0 ? 0 : (float)Math.Round((planO.ActualWorked / planO.PlanTarget) * 100, 2);
                                    planOq.Add(planO);
                                }
                                if (ReportBy == reporttype.Quarter.ToString())
                                {



                                    for (int i = 0; i < 12; i += 3)
                                    {


                                        PlanOcc planO = new PlanOcc();
                                        planO.PlanTarget = planOq[i].PlanTarget + planOq[i + 1].PlanTarget + planOq[i + 2].PlanTarget;
                                        planO.ActualWorked = planOq[i].ActualWorked + planOq[i + 1].ActualWorked + planOq[i + 2].ActualWorked;
                                        planO.Percentile = planO.PlanTarget == 0 ? 0 : (float)Math.Round((planO.ActualWorked / planO.PlanTarget) * 100, 2);
                                        planOccs.Add(planO);
                                    }


                                }
                                else
                                {
                                    planOccs = planOq;

                                }
                                plns.PlanDivision = planOccs;
                                progresseReportByStructure.plansLsts.Add(plns);

                            }
                        }
                    }

                    //  progresseReportByStructure.plansLsts = plansLsts;
                }
                return progresseReportByStructure;
            }
            catch (Exception e)
            {

                return progresseReportByStructure;
            }

        }


        public async Task<PerfomanceReport> PerformanceReports(FilterationCriteria filterationCriteria)
        {

            var performanceReport = new PerfomanceReport();

            performanceReport.performancePlan = new List<PerformancePlan>();
            try
            {

                if (filterationCriteria.structureId != null)
                {
                    List<PerformancePlan> performancePlans = new List<PerformancePlan>();
                    var BudgetYear = _dBContext.BudgetYears.Single(x => x.Year == filterationCriteria.budgetYear);

                    var ActualWorked = _dBContext.ActivityProgresses
                        .Include(x => x.Activity).ThenInclude(x => x.ActivityParent).ThenInclude(x => x.Task).ThenInclude(x => x.Plan).ThenInclude(x => x.Program)
                        .Include(x => x.Activity).ThenInclude(x => x.Plan).ThenInclude(x => x.Program)
                        .Include(x => x.Activity).ThenInclude(x => x.Task).ThenInclude(x => x.Plan).ThenInclude(x => x.Program)
                        .Include(x => x.Quarter)

                        .Where(x => x.CreatedAt >= BudgetYear.FromDate && x.CreatedAt <= BudgetYear.ToDate &&


                   (x.Activity.ActivityParentId != null ? (x.Activity.ActivityParent.Task.Plan.StructureId) : (x.Activity.TaskId != null ? x.Activity.Task.Plan.StructureId : x.Activity.Plan.StructureId))
                == filterationCriteria.structureId).ToList();

                    if (filterationCriteria.filterbyId == 1)
                    {
                        DateTime begin = DateTime.Now;
                        DateTime end = DateTime.Now;
                        if (filterationCriteria.Quarter == 1)
                        {
                            begin = EthiopicDateTime.GetGregorianDate(1, 11, filterationCriteria.budgetYear - 1);
                            end = EthiopicDateTime.GetGregorianDate(30, 1, filterationCriteria.budgetYear);
                        }
                        else if (filterationCriteria.Quarter == 2)
                        {

                            begin = EthiopicDateTime.GetGregorianDate(1, 2, filterationCriteria.budgetYear);
                            end = EthiopicDateTime.GetGregorianDate(30, 4, filterationCriteria.budgetYear);

                        }
                        else if (filterationCriteria.Quarter == 3)
                        {

                            begin = EthiopicDateTime.GetGregorianDate(1, 5, filterationCriteria.budgetYear);
                            end = EthiopicDateTime.GetGregorianDate(30, 7, filterationCriteria.budgetYear);
                        }
                        else if (filterationCriteria.Quarter == 4)
                        {

                            begin = EthiopicDateTime.GetGregorianDate(1, 8, filterationCriteria.budgetYear);
                            end = EthiopicDateTime.GetGregorianDate(30, 10, filterationCriteria.budgetYear);
                        }
                        ActualWorked = ActualWorked.Where(x => x.CreatedAt >= begin && x.CreatedAt <= end).ToList();
                    }
                    else if (filterationCriteria.filterbyId == 2)
                    {



                        int month = (int)(filterationCriteria.Month != null ? filterationCriteria.Month : 1);
                        int fromMonth = EthiopicDateTime.GetGregorianMonth(DateTime.Now.Day, month, DateTime.Now.Year);
                        ActualWorked = ActualWorked.Where(x => x.CreatedAt.Month == fromMonth).ToList();
                    }
                    else if (filterationCriteria.filterbyId == 3)
                    {
                        string[] fromDate = filterationCriteria.FromDate.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                        DateTime FromDateG = Convert.ToDateTime(EthiopicDateTime.GetGregorianDate(Int32.Parse(fromDate[1]), Int32.Parse(fromDate[0]), Int32.Parse(fromDate[2])));
                        string[] toDate = filterationCriteria.ToDate.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                        var ToDateG = Convert.ToDateTime(EthiopicDateTime.GetGregorianDate(Int32.Parse(toDate[1]), Int32.Parse(toDate[0]), Int32.Parse(toDate[2])));
                        ActualWorked = ActualWorked.Where(x => x.CreatedAt.Date >= FromDateG.Date && x.CreatedAt.Date <= ToDateG.Date).ToList();
                    }

                    // performanceReport.planDuration = ActualWorked.Select(x => x.Activity.targetDivision.ToString()).FirstOrDefault();



                    List<QuarterMonth> QuarterMonth = new List<QuarterMonth>();

                    if (filterationCriteria.reporttype == reporttype.Quarter.ToString())
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            var quar = _dBContext.QuarterSettings.Single(x => x.QuarterOrder == i);
                            //DateTime fromG = Convert.ToDateTime(EthiopicDateTime.GetGregorianDate(9, quar.StartMonth, 1984));
                            //DateTime toG = Convert.ToDateTime(EthiopicDateTime.GetGregorianDate(9, quar.EndMonth, 1984));


                            if (quar.StartMonth > 4)
                            {
                                quar.StartMonth = quar.StartMonth - 4;
                            }

                            else
                            {
                                quar.StartMonth = quar.StartMonth + 8;
                            }


                            if (quar.EndMonth > 4)
                            {
                                quar.EndMonth = quar.EndMonth - 4;
                            }

                            else
                            {
                                quar.EndMonth = quar.EndMonth + 8;
                            }



                            System.Globalization.DateTimeFormatInfo mfi = new
                                System.Globalization.DateTimeFormatInfo();
                            string fromG = mfi.GetMonthName(quar.StartMonth).ToString();

                            string toG = mfi.GetMonthName(quar.EndMonth).ToString();




                            QuarterMonth quarterMonths = new QuarterMonth();
                            quarterMonths.MonthName = "Quarter " + (i + 1);
                            QuarterMonth.Add(quarterMonths);
                        }

                    }
                    else
                    {
                        for (int i = 1; i <= 12; i++)
                        {
                            int h = 0;
                            if (i >= 7)
                            {
                                h = i - 6;
                            }

                            else
                            {
                                h = i + 6;
                            }

                            System.Globalization.DateTimeFormatInfo mfi = new
                                System.Globalization.DateTimeFormatInfo();
                            string strMonthName = mfi.GetMonthName(h).ToString();
                            //int fromG = EthiopicDateTime.GetGregorianMonth(9, k, 1984);
                            //DateTime date = new DateTime(1984, fromG, 9);
                            QuarterMonth quarterMonths = new QuarterMonth();
                            quarterMonths.MonthName = strMonthName;
                            QuarterMonth.Add(quarterMonths);
                        }

                    }


                    foreach (var items in ActualWorked)
                    {
                        PerformancePlan pp = new PerformancePlan();
                        pp.ActualWorked = items.ActualWorked;
                        pp.ReportDate = EthiopicDateTime.GetEthiopicDate(items.CreatedAt.Day, items.CreatedAt.Month, items.CreatedAt.Year);
                        pp.reportQuarter = items.Quarter.Order;
                        pp.Target = items.Quarter.Target;
                        pp.ActivityName = items.Activity.ActivityDescription == null ? items.Activity.ActivityParent.ActivityParentDescription : items.Activity.ActivityDescription;

                        if (items.Activity.ActivityParentId != null)
                        {
                            pp.TaskName = items.Activity.ActivityParent.Task.TaskDescription;
                            pp.PlanName = items.Activity.ActivityParent.Task.Plan.PlanName;
                            pp.ProgramName = items.Activity.ActivityParent.Task.Plan.Program.ProgramName;
                        }
                        else if (items.Activity.TaskId != null)
                        {
                            pp.TaskName = items.Activity.Task.TaskDescription;
                            pp.PlanName = items.Activity.Task.Plan.PlanName;
                            pp.ProgramName = items.Activity.Task.Plan.Program.ProgramName;
                        }
                        else
                        {
                            pp.TaskName = "--------";
                            pp.PlanName = items.Activity.Plan.PlanName;
                            pp.ProgramName = items.Activity.Plan.Program.ProgramName;
                        }

                        //pp.TaskName = items.Activity.Task.TaskDescription;
                        //pp.PlanName = items.Activity.Task.Plan.PlanName;
                        //pp.ProgramName = items.Activity.Task.Plan.Program.ProgramName;
                        pp.ActivityId = items.ActivityId;
                        performancePlans.Add(pp);

                        if (filterationCriteria.reporttype == reporttype.Quarter.ToString())
                        {
                            for (var i = 1; i <= 4; i++)
                            {
                                if (i == pp.reportQuarter)
                                    pp.plannedtime = QuarterMonth[i - 1].MonthName;
                            }

                        }
                        else if (filterationCriteria.reporttype == reporttype.Monthly.ToString())
                        {
                            for (var i = 1; i <= 12; i++)
                            {
                                if (i == pp.reportQuarter)
                                    pp.plannedtime = QuarterMonth[i - 1].MonthName;
                            }

                        }



                    }
                    performanceReport.performancePlan = performancePlans;
                }

                return performanceReport;

            }
            catch (Exception e)
            {

                return performanceReport;
            }



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


        public async Task<List<EstimatedCostDto>> GetEstimatedCost(Guid structureId, int budegtYear)
        {




            List<EstimatedCostDto> estimatedCosts = new List<EstimatedCostDto>();
            var plans = await _dBContext.Plans
                .Include(x => x.Tasks).ThenInclude(x => x.ActivitiesParents).ThenInclude(x => x.Activities).ThenInclude(x => x.ActProgress)
                             .Include(x => x.Tasks).ThenInclude(x => x.Activities).ThenInclude(x => x.ActProgress)
                             .Include(x => x.Activities).ThenInclude(x => x.ActProgress)
                             .Include(x => x.Tasks).ThenInclude(x => x.ActivitiesParents).ThenInclude(x => x.Activities).ThenInclude(x => x.ActivityTargetDivisions)
                             .Include(x => x.Tasks).ThenInclude(x => x.Activities).ThenInclude(x => x.ActivityTargetDivisions)
                             .Include(x => x.Activities).ThenInclude(x => x.ActivityTargetDivisions)

                .Where(x => x.StructureId == structureId && x.BudgetYear.Year == budegtYear)
                .ToListAsync();

            foreach (var item in plans)
            {
                var estimatedCost = new EstimatedCostDto();
                if (item.Activities.Any())
                {
                    estimatedCost.Description = item.Activities.First().ActivityDescription;
                    string[] GetP = GetPlanDateVariance(item.Activities.First().ShouldStat, item.Activities.First().ShouldEnd);
                    int Month = Convert.ToInt32(GetP[1]);
                    int Day = Convert.ToInt32(GetP[0]);
                    string[] GetA = new string[2];
                    if (item.Activities.First().ActualStart != null && item.Activities.First().ActualEnd != null)
                    {
                        GetA = GetActualDateVariance(Day, Month, item.Activities.First().ShouldStat, item.Activities.First().ShouldEnd);
                    }
                    estimatedCost.BudgetHours = GetP[2];
                    estimatedCost.ActualHours = GetA[0];
                    estimatedCost.HourVariance = GetA[1];
                    estimatedCost.PlannedBudjet = item.Activities.First().PlanedBudget + " ETB";
                    estimatedCost.ActualBudget = item.Activities.First().ActualBudget + " ETB";
                    estimatedCost.BudgetVariance = (item.Activities?.First()?.PlanedBudget - item.Activities?.First()?.ActualBudget).ToString() + " ETB";
                    estimatedCosts.Add(estimatedCost);
                }
                else
                {
                    estimatedCost.Description = item.PlanName;
                    estimatedCost.Tasks = new List<EstimatedCostDto>();
                    if (item.Tasks.Any())
                    {
                        foreach (var taskItems in item.Tasks)
                        {
                            var taskestimatedCost = new EstimatedCostDto();
                            if (taskItems.Activities.Any())
                            {
                                taskestimatedCost.Description = taskItems.Activities.First().ActivityDescription;
                                string[] GetP = GetPlanDateVariance(taskItems.Activities.First().ShouldStat, taskItems.Activities.First().ShouldEnd);
                                int Month = Convert.ToInt32(GetP[1]);
                                int Day = Convert.ToInt32(GetP[0]);
                                string[] GetA = new string[2];
                                if (taskItems.Activities.First().ActualStart != null && taskItems.Activities.First().ActualEnd != null)
                                {
                                    GetA = GetActualDateVariance(Day, Month, taskItems.Activities.First().ShouldStat, taskItems.Activities.First().ShouldEnd);
                                }
                                taskestimatedCost.BudgetHours = GetP[2];
                                taskestimatedCost.ActualHours = GetA[0];
                                taskestimatedCost.HourVariance = GetA[1];
                                taskestimatedCost.PlannedBudjet = taskItems.Activities.First().PlanedBudget + " ETB";
                                taskestimatedCost.ActualBudget = taskItems.Activities.First().ActualBudget + " ETB";
                                taskestimatedCost.BudgetVariance = (taskItems.Activities?.First()?.PlanedBudget - taskItems.Activities?.First()?.ActualBudget).ToString() + " ETB";
                                estimatedCost.Tasks.Add(taskestimatedCost);
                            }

                            else if (taskItems.ActivitiesParents.Any())
                            {
                                taskestimatedCost.Description = taskItems.TaskDescription;
                                taskestimatedCost.Tasks = new List<EstimatedCostDto>();

                                foreach (var PactItesm in taskItems.ActivitiesParents)
                                {
                                    var actPestimatedCost = new EstimatedCostDto();
                                    actPestimatedCost.Tasks = new List<EstimatedCostDto>();
                                    if (PactItesm.Activities.Count > 0)
                                    {
                                        actPestimatedCost.Description = PactItesm.Activities.First().ActivityDescription;
                                        string[] GetP = GetPlanDateVariance(PactItesm.Activities.First().ShouldStat, PactItesm.Activities.First().ShouldEnd);
                                        int Month = Convert.ToInt32(GetP[1]);
                                        int Day = Convert.ToInt32(GetP[0]);
                                        string[] GetA = new string[2];
                                        if (PactItesm.Activities.First().ActualStart != null && PactItesm.Activities.First().ActualEnd != null)
                                        {
                                            GetA = GetActualDateVariance(Day, Month, PactItesm.Activities.First().ShouldStat, PactItesm.Activities.First().ShouldEnd);
                                        }
                                        actPestimatedCost.BudgetHours = GetP[2];
                                        actPestimatedCost.ActualHours = GetA[0];
                                        actPestimatedCost.HourVariance = GetA[1];
                                        actPestimatedCost.PlannedBudjet = PactItesm.Activities.First().PlanedBudget + " ETB";
                                        actPestimatedCost.ActualBudget = PactItesm.Activities.First().ActualBudget + " ETB";
                                        actPestimatedCost.BudgetVariance = (PactItesm.Activities?.First()?.PlanedBudget - PactItesm.Activities?.First()?.ActualBudget).ToString() + " ETB";
                                        taskestimatedCost.Tasks.Add(actPestimatedCost);
                                    }
                                    else
                                    {

                                        actPestimatedCost.Description = PactItesm.ActivityParentDescription;

                                        foreach (var subAct in PactItesm.Activities)
                                        {
                                            var actestimatedCost = new EstimatedCostDto();
                                            actestimatedCost.Description = PactItesm.Activities.First().ActivityDescription;
                                            string[] GetP = GetPlanDateVariance(PactItesm.Activities.First().ShouldStat, PactItesm.Activities.First().ShouldEnd);
                                            int Month = Convert.ToInt32(GetP[1]);
                                            int Day = Convert.ToInt32(GetP[0]);
                                            string[] GetA = new string[2];
                                            if (PactItesm.Activities.First().ActualStart != null && PactItesm.Activities.First().ActualEnd != null)
                                            {
                                                GetA = GetActualDateVariance(Day, Month, PactItesm.Activities.First().ShouldStat, PactItesm.Activities.First().ShouldEnd);
                                            }
                                            actestimatedCost.BudgetHours = GetP[2];
                                            actestimatedCost.ActualHours = GetA[0];
                                            actestimatedCost.HourVariance = GetA[1];
                                            actestimatedCost.PlannedBudjet = PactItesm.Activities.First().PlanedBudget + " ETB";
                                            actestimatedCost.ActualBudget = PactItesm.Activities.First().ActualBudget + " ETB";
                                            actestimatedCost.BudgetVariance = (PactItesm.Activities?.First()?.PlanedBudget - PactItesm.Activities?.First()?.ActualBudget).ToString() + " ETB";
                                            taskestimatedCost.Tasks.Add(actestimatedCost);
                                        }
                                    }

                                    estimatedCost.Tasks.Add(taskestimatedCost);
                                }

                            }
                        }
                    }
                }
                estimatedCosts.Add(estimatedCost);
            }




            return estimatedCosts;


        }

        string[] GetPlanDateVariance(DateTime startDate, DateTime endDate)
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
        string[] GetActualDateVariance(int days, int totalmonths, DateTime startDate, DateTime endDate)
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

        public class EstimatedCostDto
        {

            public string Description { get; set; }

            public string BudgetHours { get; set; }

            public string ActualHours { get; set; }

            public string HourVariance { get; set; }

            public string PlannedBudjet { get; set; }

            public string ActualBudget { get; set; }

            public string BudgetVariance { get; set; }

            public List<EstimatedCostDto>? Tasks { get; set; }


        }


        public class ActivityProgressViewModel
        {

            public Guid ActivityId { get; set; }

            public PM_Case_Managemnt_Infrustructure.Models.PM.Activity Activity { get; set; }

            public float ActualBudget { get; set; }

            public float ActualWorked { get; set; }

            public Guid EmployeeValueId { get; set; }

            public Employee EmployeeValue { get; set; }

            public string Remark { get; set; }

            public string DocumentPath { get; set; }
            public DateTime CreatedDateTime { get; set; }
            public float Lat { get; set; }
            public float Lng { get; set; }

            public virtual ICollection<ProgressAttachment> ProgressAttachments { get; set; }
        }
        public class PerfomanceReport
        {
            public List<PerformancePlan> performancePlan { get; set; }
            public int planDuration { get; set; }





        }

        public class PerformancePlan
        {
            public Guid ActivityId { get; set; }
            public string ProgramName { get; set; }
            public string PlanName { get; set; }
            public string TaskName { get; set; }

            public string ActivityName { get; set; }

            public float Target { get; set; }

            public int reportQuarter { get; set; }
            public string ReportDate { get; set; }
            public float ActualWorked { get; set; }
            public string plannedtime { get; set; }
        }

        public class ProgresseReportByStructure
        {
            public List<PlansLst> plansLsts { get; set; }
            public string PreviousBudgetYear { get; set; }
            public int planDuration { get; set; }

            public int pMINT { get; set; }
            public List<QuarterMonth> planDuration2 { get; set; }

        }


        public class ProgresseReport
        {
            public List<ProgressReportTable> AllActivities { get; set; }
            public string reportMessage { get; set; }
            public int planDuration { get; set; }
            public List<QuarterMonth> planDuration2 { get; set; }

        }

        public class FilterationCriteria
        {

            public int budgetYear { get; set; }
            public Guid? empId { get; set; }
            public Guid? planId { get; set; }
            public Guid? taskId { get; set; }
            public Guid? actParentId { get; set; }

            public Guid? actId { get; set; }
            public Guid? structureId { get; set; }
            public int? filterbyId { get; set; }

            public int? Quarter { get; set; }
            public int? Month { get; set; }
            public string? FromDate { get; set; }
            public string? ToDate { get; set; }
            // public string? ReportBy { get; set; }

            public string reporttype { get; set; }
        }



        public float GetContribution(Guid structureId, float weight, Guid budetyearId)
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
                                if (!Activities.Any())
                                {
                                    Goal = Goal + planItems.PlanWeight;
                                }
                                foreach (var activityItems in Activities)
                                {
                                    BeginingPercent = BeginingPercent + ((activityItems.Begining * activityItems.Weight) / Activities.Sum(x => x.Weight));
                                    ActualWorkedPercent = ActualWorkedPercent + ((activityItems.ActualWorked * activityItems.Weight) / Activities.Sum(x => x.Weight));
                                    GoalPercent = GoalPercent + ((activityItems.Goal * activityItems.Weight) / Activities.Sum(x => x.Weight));

                                }
                                float taskItemsWeight = actparent.Weight == null ? 0 : (float)actparent.Weight;
                                BeginingPlan = BeginingPlan + ((BeginingPercent * taskItemsWeight) / (float)taskItems.Weight);
                                ActualPlan = ActualPlan + ((ActualWorkedPercent * taskItemsWeight) / (float)taskItems.Weight);
                                Goal = Goal + ((GoalPercent * taskItemsWeight) / (float)taskItems.Weight);
                            }



                        }
                        else
                        {
                            var Activities = taskItems.Activities;
                            float BeginingPercent = 0;
                            float ActualWorkedPercent = 0;
                            float GoalPercent = 0;
                            if (!Activities.Any())
                            {
                                Goal = Goal + planItems.PlanWeight;
                            }
                            foreach (var activityItems in Activities)
                            {
                                BeginingPercent = BeginingPercent + ((activityItems.Begining * activityItems.Weight) / Activities.Sum(x => x.Weight));
                                ActualWorkedPercent = ActualWorkedPercent + ((activityItems.ActualWorked * activityItems.Weight) / Activities.Sum(x => x.Weight));
                                GoalPercent = GoalPercent + ((activityItems.Goal * activityItems.Weight) / Activities.Sum(x => x.Weight));

                            }
                            float taskItemsWeight = taskItems.Weight == null ? 0 : (float)taskItems.Weight;
                            BeginingPlan = BeginingPlan + ((BeginingPercent * taskItemsWeight) / planItems.PlanWeight);
                            ActualPlan = ActualPlan + ((ActualWorkedPercent * taskItemsWeight) / planItems.PlanWeight);
                            Goal = Goal + ((GoalPercent * taskItemsWeight) / planItems.PlanWeight);
                        }
                    }
                }

                else
                {
                    var Activities = planItems.Activities;
                    float BeginingPercent = 0;
                    float ActualWorkedPercent = 0;
                    float GoalPercent = 0;
                    if (!Activities.Any())
                    {
                        Goal = Goal + planItems.PlanWeight;
                    }
                    foreach (var activityItems in Activities)
                    {
                        BeginingPercent = BeginingPercent + ((activityItems.Begining * activityItems.Weight) / Activities.Sum(x => x.Weight));
                        ActualWorkedPercent = ActualWorkedPercent + ((activityItems.ActualWorked * activityItems.Weight) / Activities.Sum(x => x.Weight));
                        GoalPercent = GoalPercent + ((activityItems.Goal * activityItems.Weight) / Activities.Sum(x => x.Weight));

                    }
                    //float taskItemsWeight = taskItems.Weight == null ? 0 : (float)taskItems.Weight;
                    BeginingPlan = BeginingPlan + BeginingPercent;
                    ActualPlan = ActualPlan + ActualWorkedPercent;
                    Goal = Goal + GoalPercent;

                }
                Pro_BeginingPlan = Pro_BeginingPlan + ((BeginingPlan * (float)planItems.PlanWeight) / 100);
                Pro_ActualPlan = Pro_ActualPlan + ((ActualPlan * (float)planItems.PlanWeight) / 100);
                Pro_Goal = Pro_Goal + ((Goal * (float)planItems.PlanWeight) / 100);
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
                    Progress = (Nominator / Denominator) * 100;
                }
            }
            else Progress = 0;
            //   Progress.Progress = Progress.Progress = ((ActualPlan - BeginingPlan) / (Progress.Weight - BeginingPlan)) * 100; ;


            performance = ((float)(Progress));
            contribution = (performance * weight) / 100;
            performance = (float)Math.Round(performance, 2);



            contribution = (float)Math.Round(contribution, 2);

            return contribution;
        }





        public class PlanReportByProgramDto
        {
            public List<ProgramViewModel> ProgramViewModels { get; set; }
            public List<FiscalPlanProgram> MonthCounts { get; set; }
        }
        public class ProgramViewModel
        {
            public string ProgramName { get; set; }

            public List<ProgramPlanViewModel> ProgramPlanViewModels { get; set; }
        }

        public class ProgramPlanViewModel
        {

            public string ProgramName { get; set; }

            public string PlanNAme { get; set; }

            public string MeasurementUnit { get; set; }

            public float TotalGoal { get; set; }

            public float TotalBirr { get; set; }


            public List<FiscalPlanProgram> FiscalPlanPrograms { get; set; }

        }

        public class FiscalPlanProgram
        {
            public string PlanNAme { get; set; }


            public int RowORder { get; set; }
            public string MonthName { get; set; }
            public float fisicalValue { get; set; }
            public float FinancialValue { get; set; }

        }


        //structure

        public class PlanReportDetailDto
        {
            public List<ProgramWithStructure> ProgramWithStructure { get; set; }
            public List<ActivityTargetDivisionReport> MonthCounts { get; set; }
        }
        public class ProgramWithStructure
        {
            public string StrutureName { get; set; }
            public List<StructurePlan> StructurePlans { get; set; }
        }

        public class StructurePlan
        {
            public string PlanName { get; set; }
            public float? Weight { get; set; }

            public string UnitOfMeasurement { get; set; }

            public float? Target { get; set; }
            public List<PlanTask> PlanTasks { get; set; }
            public List<ActivityTargetDivisionReport> PlanTargetDivision { get; set; }
        }

        public class PlanTask
        {
            public string TaskName { get; set; }
            public float? Weight { get; set; }

            public string UnitOfMeasurement { get; set; }

            public float? Target { get; set; }

            public List<TaskActivity> TaskActivities { get; set; }
            public List<ActivityTargetDivisionReport> TaskTargetDivision { get; set; }
        }

        public class TaskActivity
        {
            public string ActivityName { get; set; }
            public float? Weight { get; set; }

            public string UnitOfMeasurement { get; set; }

            public float? Target { get; set; }
            public List<ActSubActivity> ActSubActivity { get; set; }

            public List<ActivityTargetDivisionReport> ActivityTargetDivision { get; set; }

        }

        public class ActSubActivity
        {
            public string SubActivityDescription { get; set; }
            public float Weight { get; set; }

            public string UnitOfMeasurement { get; set; }

            public float Target { get; set; }

            public List<ActivityTargetDivisionReport> subActivityTargetDivision { get; set; }
        }

        public class ActivityTargetDivisionReport
        {
            public int Order { get; set; }
            public string MonthName { get; set; }
            public float TargetValue { get; set; }
        }
        public class QuarterMonth
        {
            public string MonthName { get; set; }


        }
        // planned report 



        public class PlannedReport
        {
            public List<PlansLst> PlansLst { get; set; }
            public int pMINT { get; set; }
            public List<QuarterMonth> planDuration { get; set; }
        }
        public class PlansLst
        {
            public string PlanName { get; set; }
            public float Weight { get; set; }
            public string PlRemark { get; set; }
            public bool HasTask { get; set; }
            public float? Begining { get; set; }
            public float? Target { get; set; }
            public float? ActualWorked { get; set; }
            public float? Progress { get; set; }
            public string MeasurementUnit { get; set; }

            public List<TaskLst> taskLsts { get; set; }
            public List<PlanOcc> PlanDivision { get; set; }
        }

        public class TaskLst
        {
            public string TaskDescription { get; set; }
            public float? TaskWeight { get; set; }
            public string TRemark { get; set; }
            public bool HasActParent { get; set; }
            public float? Begining { get; set; }
            public float? Target { get; set; }
            public float? ActualWorked { get; set; }
            public string MeasurementUnit { get; set; }
            public float? Progress { get; set; }
            public List<ActParentLst> ActParentLst { get; set; }
            public List<PlanOcc> TaskDivisions { get; set; }
        }

        public class ActParentLst
        {
            public string ActParentDescription { get; set; }
            public float? ActParentWeight { get; set; }
            public string ActpRemark { get; set; }
            public string MeasurementUnit { get; set; }

            public float? Begining { get; set; }
            public float? Target { get; set; }
            public float? ActualWorked { get; set; }
            public float? Progress { get; set; }
            public List<ActivityLst> activityLsts { get; set; }
            public List<PlanOcc> ActDivisions { get; set; }
        }
        public class ActivityLst
        {
            public string ActivityDescription { get; set; }
            public float Weight { get; set; }
            public string MeasurementUnit { get; set; }

            public float Begining { get; set; }
            public float Target { get; set; }

            public string Remark { get; set; }

            public float ActualWorked { get; set; }
            public float Progress { get; set; }

            public List<PlanOcc> Plans { get; set; }
        }

        public class PlanOcc
        {
            public float PlanTarget { get; set; }
            public float ActualWorked { get; set; }
            public float Percentile { get; set; }
        }

        public enum reporttype
        {
            Quarter,
            Monthly
        }

        // progress report 
        public class ProgressReportTable
        {
            public Guid ActivityId { get; set; }
            public string ProgramDescription { get; set; }
            public string PlanDescription { get; set; }
            public string TaskDescription { get; set; }
            public string ActivityDescription { get; set; }
            public string PlanStartDate { get; set; }
            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }
            public string PlanEndDate { get; set; }
            public float? PlannedBudget { get; set; }
            public float? UsedBudget { get; set; }
            public string ProgressStartDate { get; set; }
            public string ProgressEndDate { get; set; }
            public DateTime? ActualStartDate { get; set; }
            public DateTime? ActualEndDate { get; set; }
            public Status Status { get; set; }
            public float? Weight { get; set; }
            public float? Goal { get; set; }
            public float? Progress { get; set; }
            public List<Employee> Employees { get; set; }
            public float Begining { get; set; }
            public float ActualWorked { get; set; }

            public List<planOccurence> planOccurences { get; set; }



        }
        //  public class PlanDur
        //  {
        //      public string Name { get; set; }
        //  }
        public class planOccurence
        {
            public float Planned { get; set; }
            public float Achivement { get; set; }
            public float APercentile { get; set; }
            public int? QuarterOrder { get; set; }
        }
    }
}
