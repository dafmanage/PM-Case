using PM_Case_Managemnt_Infrustructure.Models.Common.Organization;

namespace PM_Case_Managemnt_Infrustructure.Models.Common
{
    public class OrganizationalStructure : CommonModel
    {

        public OrganizationalStructure()
        {

            //  EmployeesCollection = new HashSet<Employee>();
            SubTask = new HashSet<OrganizationalStructure>();
        }


        public Guid OrganizationBranchId { get; set; }
        //public virtual OrganizationalStructure OrganizationBranch { get; set; } = null!;



        //public Guid OrganizationProfileId { get; set; }
        //public virtual OrganizationProfile OrganizationProfile { get; set; } = null!;

        public Guid SubsidiaryOrganizationId { get; set; }
        public SubsidiaryOrganization SubsidiaryOrganization { get; set; }


        public Guid? ParentStructureId { get; set; }
        public virtual OrganizationalStructure ParentStructure { get; set; } = null!;

        public string StructureName { get; set; } = null!;

        public int Order { get; set; }

        public float Weight { get; set; }
        public ICollection<OrganizationalStructure> SubTask { get; set; }

        public bool IsBranch { get; set; }

        public string OfficeNumber { get; set; }




    }
}
