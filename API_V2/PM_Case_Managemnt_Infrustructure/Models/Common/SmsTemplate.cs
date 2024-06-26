using PM_Case_Managemnt_Infrustructure.Models.Common.Organization;

namespace PM_Case_Managemnt_Infrustructure.Models.Common
{
    public class SmsTemplate : CommonModel
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public Guid SubsidiaryOrganizationId { get; set; }
        public virtual SubsidiaryOrganization? SubsidiaryOrganization { get; set; }


    }
}
