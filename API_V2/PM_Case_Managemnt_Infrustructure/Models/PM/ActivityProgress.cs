
using PM_Case_Managemnt_Infrustructure.Models.Common;

namespace PM_Case_Managemnt_Infrustructure.Models.PM
{
    public class ActivityProgress : CommonModel
    {
        public ActivityProgress()
        {
            ProgressAttachments = new HashSet<ProgressAttachment>();
        }

        public Guid ActivityId { get; set; }
        public virtual Activity Activity { get; set; }
        public float ActualBudget { get; set; }
        public float ActualWorked { get; set; }
        public Guid EmployeeValueId { get; set; }
        public virtual Employee? EmployeeValue { get; set; }
        public Guid QuarterId { get; set; }
        public virtual ActivityTargetDivision Quarter { get; set; }
        //  public string DocumentPath { get; set; } = null!;
        public string? FinanceDocumentPath { get; set; }
        public ApprovalStatus IsApprovedByManager { get; set; }
        public ApprovalStatus IsApprovedByFinance { get; set; }
        public ApprovalStatus IsApprovedByDirector { get; set; }
        public string? FinanceApprovalRemark { get; set; }
        public string? CoordinatorApprovalRemark { get; set; }
        public string? DirectorApprovalRemark { get; set; }
        public string Lat { get; set; } = null!;
        public string Lng { get; set; } = null!;
        public ProgressStatus progressStatus { get; set; }


        public ICollection<ProgressAttachment> ProgressAttachments { get; set; }

        public Guid? CaseId { get; set; }
        public virtual PM_Case_Managemnt_Infrustructure.Models.CaseModel.CaseHistory Case { get; set; }
    }
    public enum ProgressStatus
    {
        SimpleProgress,
        Finalize
    }
    public enum ApprovalStatus
    {   
        Pending,
        Approved,
        Rejected
    }
}
