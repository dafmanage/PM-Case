using PM_Case_Managemnt_Infrustructure.Models.Common;
using PM_Case_Managemnt_Infrustructure.Models.Common.Organization;

namespace PM_Case_Managemnt_Infrustructure.Models.CaseModel
{
    public class Applicant : CommonModel
    {

        public string ApplicantName { get; set; } = null!;

        public string PhoneNumber { get; set; } = null!;


        public string Email { get; set; }

        public string CustomerIdentityNumber { get; set; } = null!;
        public virtual SubsidiaryOrganization? SubsidiaryOrganization { get; set; }
        public Guid SubsidiaryOrganizationId { get; set; }


        public ApplicantType ApplicantType { get; set; }


    }

    public enum ApplicantType
    {
        Organization,
        Indivisual
    }
}
