

namespace PM_Case_Managemnt_Infrustructure.Models.Common
{
    public class EmployeeStructures : CommonModel
    {
        public virtual Employee Employee { get; set; } = null!;

        public Guid EmployeeId { get; set; }


        public virtual OrganizationalStructure OrganizationalStructure { get; set; } = null!;

        public Guid OrganizationalStructureId { get; set; }


        public Position Position { get; set; }

    }
}
