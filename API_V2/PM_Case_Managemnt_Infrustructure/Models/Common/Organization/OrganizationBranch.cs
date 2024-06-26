using PM_Case_Managemnt_Infrustructure.Models.Common.Organization;
using System.ComponentModel;

namespace PM_Case_Managemnt_Infrustructure.Models.Common
{
    public class OrganizationBranch : CommonModel
    {
        public OrganizationBranch()
        {
            structures = new HashSet<OrganizationalStructure>();
        }

        public Guid OrganizationProfileId { get; set; }
        public virtual OrganizationProfile OrganizationProfile { get; set; } = null!;
        public string Name { get; set; } = null!;

        public string Address { get; set; } = null!;

        public string PhoneNumber { get; set; } = null!;

        [DefaultValue(false)]
        public bool IsHeadOffice { get; set; }
        public ICollection<OrganizationalStructure> structures { get; set; }


        public Guid SubsidiaryOrganizationId { get; set; }
        public virtual SubsidiaryOrganization SubsidiaryOrganization { get; set; } = null!;

    }
}
