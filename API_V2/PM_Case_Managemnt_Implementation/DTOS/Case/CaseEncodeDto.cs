using PM_Case_Managemnt_Implementation.DTOS.Common;
using PM_Case_Managemnt_Infrustructure.Models.CaseModel;

namespace PM_Case_Managemnt_Implementation.DTOS.CaseDto
{
    public class CaseTransferDto
    {
        public List<CaseAttachment>? CaseAttachments { get; set; }
        public Guid CaseHistoryId { get; set; }
        public Guid ToEmployeeId { get; set; }
        public Guid FromEmployeeId { get; set; }
        public Guid CaseTypeId { get; set; }
        public Guid ToStructureId { get; set; }
        public string Remark { get; set; }
    }

    public class CaseRevertDto : CaseCompleteDto
    {
        public Guid EmployeeId { get; set; }
    }
    public class CaseCompleteDto
    {
        public List<CaseAttachment>? CaseAttachments { get; set; }
        public Guid CaseHistoryId { get; set; }
        public Guid EmployeeId { get; set; }
        public string? Remark { get; set; }

    }
    public class CaseAssignDto
    {
        public Guid CaseId { get; set; }
        public Guid AssignedByEmployeeId { get; set; }
        public Guid? AssignedToEmployeeId { get; set; }
        public Guid AssignedToStructureId { get; set; }
        public Guid[]? ForwardedToStructureId { get; set; }
        public string? Remark { get; set; }



    }
    public class CaseIssueDto
    {
        public Guid CaseId { get; set; }
        public Guid AssignedByEmployeeId { get; set; }
        public Guid? AssignedToEmployeeId { get; set; }
        public Guid AssignedToStructureId { get; set; }
        public Guid? ForwardedToStructureId { get; set; }
        public string? Remark { get; set; }



    }
    public class CaseEncodePostDto
    {
        public Guid CaseID { get; set; }
        public string CaseNumber { get; set; }
        public string LetterNumber { get; set; }
        public string LetterSubject { get; set; }
        public Guid CaseTypeId { get; set; }
        public Guid? ApplicantId { get; set; }
        public Guid? EmployeeId { get; set; }
        public string PhoneNumber2 { get; set; }
        public string Representative { get; set; }
        public Guid CreatedBy { get; set; }
        public Guid EncoderEmpId { get; set; }
        public Guid SubsidiaryOrganizationId { get; set; }
    }
    public class CaseEncodeGetDto
    {
        public Guid Id { get; set; }
        public string CaseTypeName { get; set; }
        public string CaseNumber { get; set; }
        public string CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? ApplicantName { get; set; }
        public string? ApplicantPhoneNo { get; set; }
        public string? EmployeeName { get; set; }
        public string? EmployeePhoneNo { get; set; }
        public string LetterNumber { get; set; }
        public string LetterSubject { get; set; }
        public string? FromStructure { get; set; }
        public string? FromEmployeeId { get; set; }
        public string? ToStructure { get; set; }
        public string? ToEmployee { get; set; }
        public string? ToEmployeeId { get; set; }
        public string? ReciverType { get; set; }
        public bool? SecreateryNeeded { get; set; }
        public bool? IsConfirmedBySeretery { get; set; }
        public string? AffairHistoryStatus { get; set; }
        public string? Position { get; set; }
        public bool? IsSMSSent { get; set; }
        public string? CaseTypeId { get; set; }

        public string? FolderName { get; set; }

        public string? RowNumber { get; set; }

        public string? ShelfNumber { get; set; }

        public string? Remark { get; set; }

        public string? IssueStatus { get; set; }
        public string? AssignedTo { get; set; }
        public string? AssignedBy { get; set; }

        public bool IssueAction { get; set; }

        public string? ApplicantId { get; set; }

        public string? Representative { get; set; }

        public int? ChildOrder { get; set; }
        public List<SelectListDto>? Attachments { get; set; }

        public List<CaseDetailStructureDto>? CaseDetailStructures { get; set; }
    }

    public class CaseDetailStructureDto
    {
        public string FromEmployee { get; set; }
        public string FormStructure { get; set; }
        public string SeenDate { get; set; }
    }
    public class CaseIssueActionDto
    {

        public Guid issueCaseId { get; set; }
        public string action { get; set; }


    }
    public class CaseEncodeBaseDto
    {
        public Guid? ApplicantId { get; set; }
        public Guid? EmployeeId { get; set; }
        public string LetterNumber { get; set; }
        public string LetterSubject { get; set; }
        public Guid CaseTypeId { get; set; }
        public string PhoneNumber2 { get; set; }
        public string Representative { get; set; }
        public bool? IsArchived { get; set; }
        public bool? SMSStatus { get; set; }
        public Guid CreatedBy { get; set; }
        public string? Remark { get; set; }
    }


    public class ArchivedCaseDto
    {

        public Guid CaseId { get; set; }
        public Guid FolderId { get; set; }
    }
    public class CaseFilesGetDto
    {
        public string FileName { get; set; }
        public string FilePath { get; set; }
    }
}
