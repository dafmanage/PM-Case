using PM_Case_Managemnt_Implementation.DTOS.Common;
using PM_Case_Managemnt_Infrustructure.Models.PM;

namespace PM_Case_Managemnt_Implementation.DTOS.PM
{
    public class ActivityViewDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public float PlannedBudget { get; set; }
        public string ActivityType { get; set; } = null!;
        public float Weight { get; set; }
        public float Begining { get; set; }
        public float Target { get; set; }
        public string UnitOfMeasurment { get; set; } = null!;
        public bool AssignedToBranch { get; set; }
        public Guid? UnitOfMeasurmentId { get; set; }
        public float OverAllPerformance { get; set; }
        public string StartDate { get; set; } = null!;
        public string EndDate { get; set; } = null!;
        public List<SelectListDto> Members { get; set; } = null!;
        public List<MonthPerformanceViewDto>? MonthPerformance { get; set; } = null!;
        public float OverAllProgress { get; set; }
        public string? ProgresscreatedAt { get; set; }
        public bool IsFinance { get; set; }
        public bool IsProjectManager { get; set; }
        public bool IsDirector { get; set; }
        public Guid? BranchId { get; set; }
        public bool IsClassfiedToBranch { get; set; }
        public ProjectType? ProjectType { get; set; }
        public float? UsedBudget { get; set; }
        public float? OfficeWork { get; set; }
        public float? FieldWork { get; set; }
        public Guid? CommiteeId { get; set; }
        public string? StartDateEth { get; set; }
        public string? EndDateEth { get; set; }
        public Guid? KpiGoalId { get; set; }
        public bool HasKpiGoal { get; set; }
    }

    public class MonthPerformanceViewDto
    {
        public Guid Id { get; set; }
        public int Order { get; set; }
        public string MonthName { get; set; } = null!;
        public float Planned { get; set; }
        public float Actual { get; set; }
        public float Percentage { get; set; }
    }


    public class ActivityTargetDivisionDto
    {
        public Guid ActiviyId { get; set; }
        public Guid CreatedBy { get; set; }
        public List<TargetDivisionDto> TargetDivisionDtos { get; set; }
    }

    public class TargetDivisionDto
    {
        public int Order { get; set; }
        public float Target { get; set; }
        public float TargetBudget { get; set; }
    }


    public class AddProgressActivityDto
    {
        public Guid ActivityId { get; set; }
        public Guid QuarterId { get; set; }
        public Guid EmployeeValueId { get; set; }
        public string ProgressStatus { get; set; }
        public float ActualBudget { get; set; }
        public float ActualWorked { get; set; }
        public string? Lat { get; set; }
        public string? Lng { get; set; }
        public Guid CreatedBy { get; set; }
        public string[] DcoumentPath { get; set; }
        public string FinacncePath { get; set; }
        public string Remark { get; set; }
        public string lat { get; set; }
        public string lng { get; set; }
    }


    public class ProgressViewDto
    {
        public Guid Id { get; set; }
        public float ActalWorked { get; set; }
        public float UsedBudget { get; set; }
        public string[] Documents { get; set; }
        public string FinanceDocument { get; set; }
        public string Remark { get; set; }
        public string IsApprovedByManager { get; set; }
        public string IsApprovedByFinance { get; set; }
        public string IsApprovedByDirector { get; set; }
        public string? FinanceApprovalRemark { get; set; }
        public string? ManagerApprovalRemark { get; set; }
        public string? DirectorApprovalRemark { get; set; }
        public Guid? CaseId { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ApprovalProgressDto
    {
        public Guid ProgressId { get; set; }
        public string UserType { get; set; }
        public string Actiontype { get; set; }
        public string Remark { get; set; }
        public Guid CreatedBy { get; set; }
    }

    public class ActivityAttachmentDto
    {
        public string FilePath { get; set; }
        public string FileType { get; set; }
        public string ActivityDesctiption { get; set; }
    }

    public class ActivityEmployees
    {
        public Guid ActivityId { get; set; }
        public Guid CreatedBy { get; set; }
        public List<string> Employees { get; set; }
    }


}