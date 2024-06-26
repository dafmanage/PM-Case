using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace PM_Case_Managemnt_Infrustructure.Models.Auth
{
    public class ApplicationUser : IdentityUser
    {
        [Column(TypeName = "nvarchar(150)")]
        public string FullName { get; set; }
        public Guid EmployeesId { get; set; }
        public Guid SubsidiaryOrganizationId { get; set; }




    }


}
