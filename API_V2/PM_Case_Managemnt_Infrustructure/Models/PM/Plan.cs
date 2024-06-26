
using PM_Case_Managemnt_Infrustructure.Models.Common;
using System.ComponentModel;

namespace PM_Case_Managemnt_Infrustructure.Models.PM
{
    public class Plan : CommonModel
    {
        public Plan()
        {
            Tasks = new HashSet<Task>();
            TaskMemos = new HashSet<TaskMemo>();
            TaskMember = new HashSet<TaskMembers>();
            Activities = new HashSet<Activity>();
        }

        public string PlanName { get; set; } = null!;
        public Guid? BudgetYearId { get; set; }
        public virtual BudgetYear BudgetYear { get; set; } = null!;
        public Guid StructureId { get; set; }
        public virtual OrganizationalStructure Structure { get; set; } = null!;
        public DateTime? PeriodStartAt { get; set; }
        public DateTime? PeriodEndAt { get; set; }
        public Guid ProjectManagerId { get; set; }
        public virtual Employee ProjectManager { get; set; } = null!;
        // public Guid ProjectCordinatorId { get; set; }
        public Guid? FinanceId { get; set; }
        public virtual Employee Finance { get; set; } = null!;
        public Guid? ProgramId { get; set; }
        public virtual Programs Program { get; set; } = null!;
        public float PlanWeight { get; set; }

        [DefaultValue(true)]
        public bool HasTask { get; set; }
        public float PlandBudget { get; set; }
        public ProjectType ProjectType { get; set; }
        public string? ProjectFunder { get; set; }


        public ICollection<Task> Tasks { get; set; }


        public ICollection<TaskMemo> TaskMemos { get; set; }


        public ICollection<TaskMembers> TaskMember { get; set; }


        public ICollection<Activity> Activities { get; set; }

    }

    public enum ProjectType
    {
        Capital,
        Regular
    }
}
