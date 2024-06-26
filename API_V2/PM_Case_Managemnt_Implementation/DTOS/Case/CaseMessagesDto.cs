using PM_Case_Managemnt_Infrustructure.Models.CaseModel;

namespace PM_Case_Managemnt_Implementation.DTOS.Case
{
    public class CaseMessagesPostDto
    {
        public Guid Id { get; set; }
        public Guid? CaseId { get; set; }
        public MessageFrom MessageFrom { get; set; }
        public string MessageBody { get; set; } = null!;
        public bool Messagestatus { get; set; }
        public Guid CreatedBy { get; set; }

    }
    public class CaseUnsentMessagesGetDto
    {
        public Guid? Id { get; set; }

        public string CaseNumber { get; set; }
        public string ApplicantName { get; set; }
        public string LetterNumber { get; set; }
        public string Subject { get; set; }
        public string CaseTypeTitle { get; set; }

        public string PhoneNumber { get; set; }
        public string PhoneNumber2 { get; set; }

        public string Message { get; set; }

        public string MessageGroup { get; set; }

        public bool IsSmsSent { get; set; }


    }
}
