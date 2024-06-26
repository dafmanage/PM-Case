namespace PM_Case_Managemnt_Infrustructure.Models.Auth
{
    public class ApplicationUserModel
    {
        public string UserName { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string[] Roles { get; set; } = null!;
        public Guid EmployeeId { get; set; }
        public Guid SubsidiaryOrganizationId { get; set; }



    }
}
