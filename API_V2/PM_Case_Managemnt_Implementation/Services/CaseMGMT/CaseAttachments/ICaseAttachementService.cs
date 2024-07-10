using PM_Case_Managemnt_Implementation.Helpers.Response;
using PM_Case_Managemnt_Infrustructure.Models.CaseModel;

namespace PM_Case_Managemnt_Implementation.Services.CaseMGMT.CaseAttachments

{
    public interface ICaseAttachementService
    {
        public Task<ResponseMessage<string>> AddMany(List<CaseAttachment> caseAttachments);
        public Task<ResponseMessage<List<CaseAttachment>>> GetAll(Guid subOrgId, string? CaseId = null);
        public ResponseMessage<bool> RemoveAttachment(Guid attachmentId);
    }
}
