using PM_Case_Managemnt_Implementation.DTOS.CaseDto;
using PM_Case_Managemnt_Implementation.Helpers.Response;

namespace PM_Case_Managemnt_Implementation.Services.CaseMGMT
{
    public interface ICaseProccessingService
    {
        public Task<ResponseMessage<int>> ConfirmTranasaction(ConfirmTranscationDto confirmTranscationDto);
        public Task<ResponseMessage<int>> AssignTask(CaseAssignDto caseAssignDto);
        public Task<ResponseMessage<int>> CompleteTask(CaseCompleteDto caseCompleteDto);
        public Task<ResponseMessage<int>> RevertTask(CaseRevertDto revertAffair);
        public Task<ResponseMessage<int>> TransferCase(CaseTransferDto caseTransferDto);
        public Task<ResponseMessage<int>> AddToWaiting(Guid caseHistoryId);
        public Task<ResponseMessage<CaseEncodeGetDto>> GetCaseDetial(Guid historyId, Guid employeeId);
        public Task<ResponseMessage<int>> SendSMS(CaseCompleteDto smsdetail);
        public Task<ResponseMessage<int>> ArchiveCase(ArchivedCaseDto archivedCaseDto);
        public Task<ResponseMessage<CaseState>> GetCaseState(Guid CaseTypeId, Guid caseHistoryId);
        public Task<ResponseMessage<bool>> Ispermitted(Guid employeeId, Guid caseId);
    }
}
