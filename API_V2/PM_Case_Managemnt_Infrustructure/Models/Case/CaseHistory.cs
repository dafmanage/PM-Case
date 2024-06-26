using PM_Case_Managemnt_Infrustructure.Models.Common;
using System.ComponentModel;

namespace PM_Case_Managemnt_Infrustructure.Models.CaseModel
{
    public class CaseHistory : CommonModel
    {
        public CaseHistory()
        {
            Attachments = new HashSet<CaseHistoryAttachment>();
        }

        public Guid CaseId { get; set; }
        public virtual Case Case { get; set; }
        public Guid? CaseTypeId { get; set; }
        public virtual CaseType CaseType { get; set; }
        public Guid? FromEmployeeId { get; set; }
        public virtual Employee FromEmployee { get; set; }
        public Guid? FromStructureId { get; set; }
        public virtual OrganizationalStructure FromStructure { get; set; }
        public Guid? ToEmployeeId { get; set; }
        public virtual Employee ToEmployee { get; set; }
        public Guid? ToStructureId { get; set; }
        public virtual OrganizationalStructure ToStructure { get; set; }
        public AffairHistoryStatus AffairHistoryStatus { get; set; }
        public int WaitingPeriod { get; set; }
        public DateTime? SeenDateTime { get; set; }
        public DateTime? TransferedDateTime { get; set; }
        public DateTime? CompletedDateTime { get; set; }
        public DateTime? RevertedAt { get; set; }
        public ReciverType ReciverType { get; set; }
        //public string Document { get; set; }
        public bool IsSmsSent { get; set; }
        public bool IsConfirmedBySeretery { get; set; }
        public bool IsForwardedBySeretery { get; set; }

        [DefaultValue(false)]
        public bool SecreateryNeeded { get; set; }
        public DateTime? SecreteryConfirmationDateTime { get; set; }
        public Guid? SecreteryId { get; set; }
        public virtual Employee Secretery { get; set; }
        public DateTime? ForwardedDateTime { get; set; }
        public Guid? ForwardedById { get; set; }
        public virtual Employee ForwardedBy { get; set; }


        public virtual ICollection<CaseHistoryAttachment> Attachments { get; set; }

        public int childOrder { get; set; }
    }


    public enum ReciverType
    {
        Orginal,
        Cc
    }

    public enum AffairHistoryStatus
    {
        Pend,
        Seen,
        Transfered,
        Revert,
        Completed,
        Waiting
    }
}
