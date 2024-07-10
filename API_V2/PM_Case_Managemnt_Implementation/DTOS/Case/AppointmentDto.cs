
namespace PM_Case_Managemnt_Implementation.DTOS.CaseDto
{
    public class AppointmentPostDto
    {
        public Guid CaseId { get; set; }
        public Guid EmployeeId { get; set; }
        public bool IsSmsSent { get; set; }
        public Guid CreatedBy { get; set; }
        public string Remark { get; set; }
    }

    public class AppointmentGetDto
    {
        public string Id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string badge { get; set; }
        public string date { get; set; }
        public string type { get; set; }
        public bool everyYear { get; set; }

    }
}
