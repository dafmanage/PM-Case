
using PM_Case_Managemnt_Infrustructure.Models.Common;

namespace PM_Case_Managemnt_Infrustructure.Models.PM
{
    public class EmployeesAssignedForActivities : CommonModel
    {

        public Guid EmployeeId { get; set; }
        public virtual Employee Employee { get; set; } = null!;

        public Guid ActivityId { get; set; }
        public virtual Activity Activity { get; set; } = null!;


    }
}
