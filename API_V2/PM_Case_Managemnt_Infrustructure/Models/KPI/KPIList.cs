using PM_Case_Managemnt_API.Models.KPI;
using PM_Case_Managemnt_Infrustructure.Models.Common;
using PM_Case_Managemnt_Infrustructure.Models.Common.Organization;

namespace PM_Case_Managemnt_Infrustructure.Models.KPI
{
    public class KPIList : CommonModel
    {
        public string Title { get; set; }
        public int StartYear { get; set; }
        public string ActiveYearsString { get; set; }
        public string? EncoderOrganizationName { get; set; }
        public string? EvaluatorOrganizationName { get; set; }
        public string? Url { get; set; }
        public bool HasSubsidiaryOrganization { get; set; }
        public Guid? SubsidiaryOrganizationId { get; set; }
        public virtual SubsidiaryOrganization? SubsidiaryOrganization { get; set; }
        public string? AccessCode { get; set; }
        public List<KPIDetails>? KPIDetails { get; set; }


    }
}
