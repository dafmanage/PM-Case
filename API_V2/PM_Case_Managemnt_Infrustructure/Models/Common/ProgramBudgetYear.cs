

using PM_Case_Managemnt_Infrustructure.Models.Common.Organization;

namespace PM_Case_Managemnt_Infrustructure.Models.Common
{
    public class ProgramBudgetYear : CommonModel
    {
        public string Name { get; set; } = null!;

        public int FromYear { get; set; }

        public int ToYear { get; set; }

        public virtual ICollection<BudgetYear>? BudgetYears { get; set; } = null!;
        public virtual SubsidiaryOrganization? SubsidiaryOrganization { get; set; }
        public Guid SubsidiaryOrganizationId { get; set; }

    }
}
