

namespace PM_Case_Managemnt_Infrustructure.Models.Common
{
    public class Employee : CommonModel
    {
        public string Photo { get; set; } = null!;

        public string Title { get; set; } = null!;

        public string FullName { get; set; } = null!;

        public Gender Gender { get; set; }

        public string PhoneNumber { get; set; } = null!;

        public virtual OrganizationalStructure OrganizationalStructure { get; set; } = null!;

        public Guid OrganizationalStructureId { get; set; }

        public Position Position { get; set; }

        public string MobileUsersMacaddress { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }

    public enum Gender
    {
        Male,
        Female
    }

    public enum Position
    {
        Director,
        Secertary,
        Expert
    }
}
