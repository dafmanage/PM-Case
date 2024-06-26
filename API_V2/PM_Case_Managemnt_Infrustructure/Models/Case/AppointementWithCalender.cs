using PM_Case_Managemnt_Infrustructure.Models.Common;

namespace PM_Case_Managemnt_Infrustructure.Models.CaseModel
{
    public class AppointementWithCalender : CommonModel
    {

        public Guid EmployeeId { get; set; }
        public virtual Employee Employee { get; set; } = null!;
        public string AppointementDate { get; set; }
        public string Time { get; set; } = null!;
        public Guid CaseId { get; set; }
        public virtual Case Case { get; set; } = null!;

    }
}
