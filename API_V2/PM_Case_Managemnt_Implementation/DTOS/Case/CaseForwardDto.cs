namespace PM_Case_Managemnt_Implementation.DTOS.CaseDto
{
    public class CaseForwardPostDto
    {
        public Guid CaseId { get; set; }
        public Guid ForwardedByEmployeeId { get; set; }
        public Guid[] ForwardedToStructureId { get; set; }
        public Guid CreatedBy { get; set; }

    }



    public class ConfirmTranscationDto
    {
        public Guid EmployeeId { get; set; }

        public Guid CaseHistoryId { get; set; }
    }
}
