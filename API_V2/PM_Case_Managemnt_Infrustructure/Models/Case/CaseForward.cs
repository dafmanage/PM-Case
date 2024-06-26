using PM_Case_Managemnt_Infrustructure.Models.Common;

namespace PM_Case_Managemnt_Infrustructure.Models.CaseModel
{
    public class CaseForward : CommonModel
    {
        public Guid CaseId { get; set; }
        public virtual Case Case { get; set; }
        public Guid ForwardedByEmployeeId { get; set; }
        public virtual Employee ForwardedByEmployee { get; set; }
        public Guid ForwardedToStructureId { get; set; }
        public virtual OrganizationalStructure ForwardedToStructure { get; set; }
    }
}
