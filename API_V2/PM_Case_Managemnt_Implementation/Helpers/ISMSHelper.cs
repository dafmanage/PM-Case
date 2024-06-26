using PM_Case_Managemnt_Infrustructure.Models.CaseModel;

namespace PM_Case_Managemnt_Implementation.Helpers
{
    public interface ISMSHelper
    {
        public Task<bool> MessageSender(string reciver, string message, string UserId, Guid? orgId = null);
        public Task<bool> UnlimittedMessageSender(string reciver, string message, string UserId, Guid? orgId = null);
        public Task<bool> SendSmsForCase(string message, Guid caseId, Guid caseHistoryId, string userId, MessageFrom messageFrom);

    }
}
