using PM_Case_Managemnt_Implementation.DTOS.Case;

namespace PM_Case_Managemnt_Implementation.Services.CaseMGMT.CaseMessagesService
{
    public interface ICaseMessagesService
    {
        public Task Add(CaseMessagesPostDto caseMessagesPost);
        public Task<List<CaseUnsentMessagesGetDto>> GetMany(Guid subOrgId, bool MessageStatus);
        public Task SendMessages(List<CaseUnsentMessagesGetDto> Messages);
    }
}