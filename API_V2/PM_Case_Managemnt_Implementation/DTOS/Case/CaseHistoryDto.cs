using PM_Case_Managemnt_Infrustructure.Models.CaseModel;

namespace PM_Case_Managemnt_Implementation.DTOS.CaseDto
{
    public class CaseHistoryPostDto
    {
        public Guid CaseId { get; set; }
        public Guid? CaseTypeId { get; set; }
        public Guid? FromEmployeeId { get; set; }
        public Guid? FromStructureId { get; set; }
        public Guid? ToEmployeeId { get; set; }
        public Guid? ToStructureId { get; set; }
        public AffairHistoryStatus AffairHistoryStatus { get; set; }
        public DateTime? SeenDateTime { get; set; }
        public DateTime? TransferedDateTime { get; set; }
        public DateTime? CompletedDateTime { get; set; }
        public DateTime? RevertedAt { get; set; }
        public ReciverType ReciverType { get; set; }
        //public string Document { get; set; }
        public bool IsSmsSent { get; set; }
        public bool IsConfirmedBySeretery { get; set; }
        public bool IsForwardedBySeretery { get; set; }
        public DateTime? SecreteryConfirmationDateTime { get; set; }
        public Guid? SecreteryId { get; set; }
        public DateTime? ForwardedDateTime { get; set; }
        public Guid? ForwardedById { get; set; }
        public Guid CreatedBy { get; set; }
    }

    public class CaseHistorySeenDto
    {
        public Guid CaseId { get; set; }
        public Guid SeenBy { get; set; }
    }

    public class CaseHistoryCompleteDto
    {
        public string Remark { get; set; }
        public Guid CompleatedBy { get; set; }
        public Guid CaseId { get; set; }
    }
}
