using PM_Case_Managemnt_Implementation.DTOS.CaseDto;


namespace PM_Case_Managemnt_Implementation.Services.CaseMGMT
{
    public interface ICaseIssueService
    {
        public Task<List<CaseEncodeGetDto>> GetNotCompletedCases(Guid subOrgId);
        public Task IssueCase(CaseIssueDto caseAssignDto);
        public Task<List<CaseEncodeGetDto>> GetAll(Guid? employeeId);
        public Task TakeAction(CaseIssueActionDto caseActionDto);
        
    }
}
