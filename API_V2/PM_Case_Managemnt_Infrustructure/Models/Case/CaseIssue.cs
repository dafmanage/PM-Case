using PM_Case_Managemnt_Infrustructure.Models.Common;

namespace PM_Case_Managemnt_Infrustructure.Models.Case
{
    public class CaseIssue : CommonModel
    {
        public Guid CaseId { get; set; }
        public virtual PM_Case_Managemnt_Infrustructure.Models.CaseModel.Case Case { get; set; }

        public Guid AssignedByEmployeeId { get; set; }
        public virtual Employee AssignedByEmployee { get; set; }

        public Guid? AssignedToEmployeeId { get; set; }
        public virtual Employee AssignedToEmployee { get; set; }
        public Guid AssignedToStructureId { get; set; }
        public virtual OrganizationalStructure AssignedToStructure { get; set; }
        public Guid? ForwardedToEmployeeId { get; set; }
        public virtual Employee ForwardedToEmployee { get; set; }
        public IssueStatus IssueStatus { get; set; }
    }

    public enum IssueStatus
    {

        Assigned,
        Rejected,
        Completed,
    }
}
