
using PM_Case_Managemnt_Infrustructure.Models.Common;

namespace PM_Case_Managemnt_Infrustructure.Models.PM
{
    public class TaskMemo : CommonModel
    {
        public TaskMemo()
        {
            Replies = new HashSet<TaskMemoReply>();
        }
        public Guid? TaskId { get; set; }
        public virtual Task Task { get; set; } = null!;
        public Guid? PlanId { get; set; }
        public virtual Plan Plan { get; set; } = null!;
        public Guid? ActivityParentId { get; set; }
        public virtual ActivityParent ActivityParent { get; set; } = null!;
        public Guid EmployeeId { get; set; }
        public virtual Employee Employee { get; set; } = null!;
        public string Description { get; set; } = null!;


        public virtual ICollection<TaskMemoReply> Replies { get; set; }
    }
}
