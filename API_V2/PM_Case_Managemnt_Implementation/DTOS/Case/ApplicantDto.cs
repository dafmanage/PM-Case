using PM_Case_Managemnt_Infrustructure.Models.Common;

namespace PM_Case_Managemnt_Implementation.DTOS.CaseDto
{
    public class ApplicantPostDto
    {
        public Guid ApplicantId { get; set; }
        public string ApplicantName { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string Email { get; set; }
        public string CustomerIdentityNumber { get; set; } = null!;
        public string ApplicantType { get; set; }
        public Guid CreatedBy { get; set; }
        public Guid SubsidiaryOrganizationId { get; set; }

    }

    public class ApplicantGetDto : ApplicantPostDto
    {
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public RowStatus RowStatus { get; set; }

    }
}
