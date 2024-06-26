using PM_Case_Managemnt_Infrustructure.Models.CaseModel;

namespace PM_Case_Managemnt_Implementation.Services.CaseMGMT.CaseAttachments
{
    public interface ICaseAttachementService
    {
        public Task AddMany(List<CaseAttachment> caseAttachments);
        public Task<List<CaseAttachment>> GetAll(Guid subOrgId, string CaseId = null);

        public bool RemoveAttachment(Guid attachmentId);
    }
}
