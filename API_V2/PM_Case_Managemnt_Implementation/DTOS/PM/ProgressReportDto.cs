using PM_Case_Managemnt_Infrustructure.Models.PM;
using PM_Case_Managemnt_Infrustructure.Models.Common;

namespace PM_Case_Managemnt_Implementation.DTOS.PM
{
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

        public class PlannedReport
        {
            public List<PlansList> PlansLists { get; set; }
            public int PMINT { get; set; }
            public List<QuarterMonth> PlanDurationInQuarter { get; set; }
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
            public string PlanName { get; set; }
            public string MeasurementUnit { get; set; }
            public float TotalGoal { get; set; }
            public float TotalBirr { get; set; }
            public List<FiscalPlanProgram> FiscalPlanPrograms { get; set; }
        }

        public class FiscalPlanProgram
        {
            public string PlanName { get; set; }
            public string MeasurementName { get; set; }
            public int RowOrder { get; set; }
            public string MonthName { get; set; }
            public float FiscalValue { get; set; }
            public float FinancialValue { get; set; }
        }

        public class PlanReportDetailDto
        {
            public List<ProgramWithStructure> ProgramWithStructure { get; set; }
            public List<ActivityTargetDivisionReport> MonthCounts { get; set; }
        }

        public class ProgramWithStructure
        {
            public string StructureName { get; set; }
            public List<StructurePlan> StructurePlans { get; set; }
        }

        public class ActivityTargetDivisionReport
        {
            public int Order { get; set; }
            public string MonthName { get; set; }
            public float TargetValue { get; set; }
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

        public class ProgressReport
        {
            public List<ProgressReportTable> AllActivities { get; set; }
            public string ReportMessage { get; set; }
            public int PlanDuration { get; set; }
            public List<QuarterMonth> PlanDurationInQuarter { get; set; }
        }

        public class QuarterMonth
        {
            public string MonthName { get; set; }
        }

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
            public List<PlanAchievement> PlanAchievement { get; set; }
        }

        public class PlanAchievemenurence
        {
            public float Planned { get; set; }
            public float Achivement { get; set; }
            public float Percentile { get; set; }
            public int? QuarterOrder { get; set; }
        }

        public class ProgresseReportByStructure
        {
            public List<PlansList> PlansLists { get; set; }
            public string PreviousBudgetYear { get; set; }
            public int PlanDuration { get; set; }
            public int PMINT { get; set; }
            public List<QuarterMonth> PlanDurationInQuarter { get; set; }

        }

        public class PlansList
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
            public List<TaskList> TaskLists { get; set; }
            public List<PlanAchievement> PlanDivision { get; set; }
        }

        public class TaskList
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
            public List<PlanAchievement> TaskDivisions { get; set; }
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
            public List<ActivityList> ActivityLists { get; set; }
            public List<PlanAchievement> ActDivisions { get; set; }
        }
        public class ActivityList
        {
            public string ActivityDescription { get; set; }
            public float Weight { get; set; }
            public string MeasurementUnit { get; set; }
            public float Begining { get; set; }
            public float Target { get; set; }
            public string Remark { get; set; }
            public float ActualWorked { get; set; }
            public float Progress { get; set; }
            public List<PlanAchievement> Plans { get; set; }
        }
        
        public class PlanAchievement
        {
            public float Target { get; set; }
            public float Actual { get; set; }
            public float PercentageAchieved { get; set; }
        }

        public class PerformanceReport
        {
            public List<PerformancePlan> PerformancePlan { get; set; }
            public int PlanDuration { get; set; }
        }

        public class PerformancePlan
        {
            public Guid ActivityId { get; set; }
            public string ProgramName { get; set; }
            public string PlanName { get; set; }
            public string TaskName { get; set; }
            public string ActivityName { get; set; }
            public float Target { get; set; }
            public int ReportQuarter { get; set; }
            public string ReportDate { get; set; }
            public float ActualWorked { get; set; }
            public string Plannedtime { get; set; }
        }

        public class ActivityProgressViewModel
        {
            public Guid ActivityId { get; set; }
            public Activity Activity { get; set; }
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

        public class FilterationCriteria
        {
            public int BudgetYear { get; set; }
            public Guid? EmpId { get; set; }
            public Guid PlanId { get; set; }
            public Guid TaskId { get; set; }
            public Guid? ActParentId { get; set; }
            public Guid? ActId { get; set; }
            public Guid? StructureId { get; set; }
            public int? FilterbyId { get; set;}
            public int? Quarter { get; set; }
            public int? Month { get; set; }
            public string? FromDate { get; set; }
            public string? ToDate { get; set; }
            public string ReportType { get; set; }
        }

}