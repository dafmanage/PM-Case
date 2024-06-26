namespace PM_Case_Managemnt_Implementation.DTOS.Common
{
    public class SelectListDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string? Photo { get; set; }

        public string? EmployeeId { get; set; }

        public string? CommiteeStatus { get; set; }
    }


    public class SelectRolesListDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public class UserModel
    {

        public string EmployeeFullName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

        public Guid EmployeeId { get; set; }

        public string[] Roles { get; set; }


    }

}
