using PM_Case_Managemnt_Infrustructure.Models.Common;

namespace PM_Case_Managemnt_Infrustructure.Models.CaseModel
{
    public class Appointement : CommonModel
    {
        public Guid CaseId { get; set; }
        public virtual Case Case { get; set; } = null!;
        public Guid EmployeeId { get; set; }
        public virtual Employee Employee { get; set; } = null!;
        public bool IsSmsSent { get; set; }
    }
}
