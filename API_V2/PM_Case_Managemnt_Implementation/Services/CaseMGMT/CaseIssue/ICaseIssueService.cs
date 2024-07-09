using PM_Case_Managemnt_Implementation.DTOS.CaseDto;
using PM_Case_Managemnt_Implementation.Helpers.Response;


namespace PM_Case_Managemnt_Implementation.Services.CaseMGMT
{
    public interface ICaseIssueService
    {
        public Task<ResponseMessage<List<CaseEncodeGetDto>>> GetNotCompletedCases(Guid subOrgId);
        public Task<ResponseMessage<string>> IssueCase(CaseIssueDto caseAssignDto);
        public Task<ResponseMessage<List<CaseEncodeGetDto>>> GetAll(Guid? employeeId);
        public Task<ResponseMessage<string>> TakeAction(CaseIssueActionDto caseActionDto);
    }
}
