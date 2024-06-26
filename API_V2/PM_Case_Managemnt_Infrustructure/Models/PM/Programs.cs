
using PM_Case_Managemnt_Infrustructure.Models.Common;
using PM_Case_Managemnt_Infrustructure.Models.Common.Organization;

namespace PM_Case_Managemnt_Infrustructure.Models.PM
{
    public class Programs : CommonModel
    {

        public string ProgramName { get; set; } = null!;
        public float ProgramPlannedBudget { get; set; }
        public Guid ProgramBudgetYearId { get; set; }
        public virtual ProgramBudgetYear? ProgramBudgetYear { get; set; }
        public virtual SubsidiaryOrganization? SubsidiaryOrganization { get; set; }
        public Guid SubsidiaryOrganizationId { get; set; }


    }
}
