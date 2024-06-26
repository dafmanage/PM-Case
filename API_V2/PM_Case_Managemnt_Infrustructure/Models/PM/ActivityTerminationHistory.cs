

using PM_Case_Managemnt_Infrustructure.Models.Common;
using System.ComponentModel;

namespace PM_Case_Managemnt_Infrustructure.Models.PM
{
    public class ActivityTerminationHistories : CommonModel
    {
        public Guid ActivityId { get; set; }
        public virtual Activity Activity { get; set; } = null!;

        public Guid FromEmployeeId { get; set; }
        public virtual Employee FromEmployee { get; set; } = null!;

        public Guid? ToEmployeeId { get; set; }
        public virtual Employee ToEmployee { get; set; } = null!;

        public Guid? ToCommiteId { get; set; }
        public virtual Commitees ToCommite { get; set; } = null!;

        public string TerminationReason { get; set; } = null!;

        public Guid ApprovedByDirectorId { get; set; }
        public virtual Employee ApprovedByDirector { get; set; } = null!;

        public string DocumentPath { get; set; } = null!;

        [DefaultValue(false)]
        public Boolean Isapproved { get; set; }
        [DefaultValue(false)]
        public Boolean IsRejected { get; set; }

    }
}
