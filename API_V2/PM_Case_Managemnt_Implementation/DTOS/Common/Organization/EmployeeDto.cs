
namespace PM_Case_Managemnt_Implementation.DTOS.Common
{
    public class EmployeeDto
    {

        public Guid? Id { get; set; }
        public string Photo { get; set; }

        public string? UserName { get; set; }
        public string?[] Roles { get; set; }
        public string Title { get; set; }

        public string FullName { get; set; }

        public string Gender { get; set; }

        public string PhoneNumber { get; set; }

        public string Position { get; set; }

        public string StructureId { get; set; }

        public string BranchId { get; set; }

        public string? BranchName { get; set; }
        public string? StructureName { get; set; }

        public string? Remark { get; set; }
        public int RowStatus { get; set; }

        public string? Password { get; set; }


    }
}
