
using PM_Case_Managemnt_Infrustructure.Models.Common;
using System.ComponentModel;

namespace PM_Case_Managemnt_Infrustructure.Models.PM
{
    public class Task : CommonModel
    {

        public Task()
        {
            TaskMemos = new HashSet<TaskMemo>();
            TaskMember = new HashSet<TaskMembers>();
            ActivitiesParents = new HashSet<ActivityParent>();
            // Activities = new HashSet<Activity>();
        }

        public Guid? PlanId { get; set; }
        public virtual Plan Plan { get; set; } = null!;
        public string TaskDescription { get; set; } = null!;
        public DateTime? ShouldStartPeriod { get; set; }
        public DateTime? ActuallStart { get; set; }
        public DateTime? ShouldEnd { get; set; }
        public DateTime? ActualEnd { get; set; }
        public float PlanedBudget { get; set; }
        public float? ActualBudget { get; set; }
        public float? Goal { get; set; }
        public float? Weight { get; set; }
        public float ActualWorked { get; set; }
        public Status Status { get; set; }
        [DefaultValue(true)]
        public bool HasActivityParent { get; set; }


        public ICollection<TaskMemo> TaskMemos { get; set; }


        public ICollection<TaskMembers> TaskMember { get; set; }


        public ICollection<ActivityParent> ActivitiesParents { get; set; }


        public ICollection<Activity> Activities { get; set; }

    }
    public enum ReportLevel
    {
        Structure,
        Employee
    }
}
