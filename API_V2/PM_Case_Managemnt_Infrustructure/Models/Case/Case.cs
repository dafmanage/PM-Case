using Microsoft.EntityFrameworkCore;
using PM_Case_Managemnt_Infrustructure.Models.Common;
using PM_Case_Managemnt_Infrustructure.Models.Common.Organization;

namespace PM_Case_Managemnt_Infrustructure.Models.CaseModel
{
    [Index(nameof(CaseNumber), IsUnique = true)]
    public class Case : CommonModel
    {

        public Case()
        {
            CaseHistories = new HashSet<CaseHistory>();
            CaseAttachments = new HashSet<CaseAttachment>();
        }

        public string CaseNumber { get; set; }

        public Guid? ApplicantId { get; set; }
        public virtual Applicant Applicant { get; set; }

        public Guid? EmployeeId { get; set; }
        public virtual Employee Employee { get; set; }

        public string LetterNumber { get; set; }
        public string LetterSubject { get; set; }

        public Guid CaseTypeId { get; set; }
        public virtual CaseType CaseType { get; set; }

        //public string? DocumentPath { get; set; }
        public DateTime? CompletedAt { get; set; }
        public AffairStatus AffairStatus { get; set; }

        public string PhoneNumber2 { get; set; }

        public string Representative { get; set; }

        public bool IsArchived { get; set; }

        public bool SMSStatus { get; set; }

        public Guid? FolderId { get; set; }

        public virtual Folder Folder { get; set; }


        public virtual ICollection<CaseHistory> CaseHistories { get; set; }


        public virtual ICollection<CaseAttachment> CaseAttachments { get; set; }

        /////
        ///
        public Guid SubsidiaryOrganizationId { get; set; }
        public virtual SubsidiaryOrganization? SubsidiaryOrganization { get; set; }
    }

    public enum AffairStatus
    {
        Pend,
        Assigned,
        Completed,
        Encoded
    }
}
