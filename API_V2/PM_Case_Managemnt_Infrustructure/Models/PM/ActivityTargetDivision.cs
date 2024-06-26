

using PM_Case_Managemnt_Infrustructure.Models.Common;

namespace PM_Case_Managemnt_Infrustructure.Models.PM
{
    public class ActivityTargetDivision : CommonModel
    {
        public Guid ActivityId { get; set; }
        public virtual Activity? Activity { get; set; } = null!;
        public float Target { get; set; }
        public float TargetBudget { get; set; }
        public int Order { get; set; }
    }

    public enum TargetDivision
    {
        Quarterly,
        Monthly
    }
}
