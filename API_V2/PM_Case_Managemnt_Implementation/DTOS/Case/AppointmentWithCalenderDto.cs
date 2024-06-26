namespace PM_Case_Managemnt_Implementation.DTOS.CaseDto
{
    public class AppointmentWithCalenderPostDto
    {
        public Guid EmployeeId { get; set; }
        public string AppointementDate { get; set; }
        public string Time { get; set; } = null!;
        public Guid CaseId { get; set; }
        public Guid CreatedBy { get; set; }
        public string? Remark { get; set; }
    }
}
