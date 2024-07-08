
using PM_Case_Managemnt_Infrustructure.Models.Common;

namespace PM_Case_Managemnt_Infrustructure.Models.PM
{
    public class ActivityParent : CommonModel
    {
        public ActivityParent()
        {
            Activities = [];
            TaskMemos = [];
            TaskMember = [];
        }
        public Guid? TaskId { get; set; }
        public virtual Task Task { get; set; }
        public string ActivityParentDescription { get; set; } = null!;
        public DateTime? ShouldStartPeriod { get; set; }
        public DateTime? ActuallStart { get; set; }
        public DateTime? ShouldEnd { get; set; }
        public DateTime? ActualEnd { get; set; }
        public float PlanedBudget { get; set; }
        public float? ActualBudget { get; set; }
        public float Goal { get; set; }
        public float Weight { get; set; }
        public float ActualWorked { get; set; }
        public bool AssignedToBranch { get; set; }
        public Status Status { get; set; }
        public bool HasActivity { get; set; }
        public bool IsClassfiedToBranch { get; set; }
        public float BaseLine { get; set; }
        public Guid? UnitOfMeasurmentId { get; set; }
        public UnitOfMeasurment UnitOfMeasurment { get; set; }
        public ICollection<Activity> Activities { get; set; }
        public ICollection<TaskMemo> TaskMemos { get; set; }
        public ICollection<TaskMembers> TaskMember { get; set; }
    }
}
