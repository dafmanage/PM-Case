namespace PM_Case_Managemnt_Infrustructure.Models.Common.Organization
{
    public class SubsidiaryOrganization : CommonModel
    {
        public SubsidiaryOrganization()
        {
            structures = new HashSet<OrganizationalStructure>();
            // branches = new HashSet<OrganizationBranch>();
        }

        public Guid OrganizationProfileId { get; set; }
        public virtual OrganizationProfile OrganizationProfile { get; set; } = null!;


        public ICollection<OrganizationalStructure> structures { get; set; }

        //public ICollection<OrganizationBranch> branches { get; set; }



        public string OrganizationNameEnglish { get; set; } = null!;

        public string OrganizationNameInLocalLanguage { get; set; } = null!;

        //public string Logo { get; set; } = null!;

        public string Address { get; set; } = null!;

        public string PhoneNumber { get; set; } = null!;

        public int SmsCode { get; set; }

        public string UserName { get; set; } = null!;

        public string Password { get; set; } = null!;

        public bool isRegulatoryBody { get; set; }
        public bool isMonitor { get; set; }



    }
}
