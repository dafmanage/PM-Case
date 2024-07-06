using PM_Case_Managemnt_Implementation.DTOS.CaseDto;
using PM_Case_Managemnt_Implementation.Helpers.Response;

namespace PM_Case_Managemnt_Implementation.Services.CaseService.Encode
{
    public interface ICaseEncodeService
    {
        public Task<ResponseMessage<string>> Add(CaseEncodePostDto caseEncodePostDto);
        public Task<ResponseMessage<string>> Update(CaseEncodePostDto caseEncodePostDto);
        public Task<ResponseMessage<List<CaseEncodeGetDto>>> GetAll(Guid userId);
        public Task<ResponseMessage<CaseEncodeGetDto>> GetSingleCase(Guid caseId);
        public Task<ResponseMessage<string>> GetCaseNumber(Guid subOrgId);
        public Task<ResponseMessage<List<CaseEncodeGetDto>>> GetAllTransfred(Guid employeeId);
        public Task<ResponseMessage<List<CaseEncodeGetDto>>> MyCaseList(Guid employeeId);

        public Task<ResponseMessage<List<CaseEncodeGetDto>>> CompletedCases(Guid subOrgId);

        public Task<ResponseMessage<List<CaseEncodeGetDto>>> GetArchivedCases(Guid subOrgId);




        public Task<ResponseMessage<List<CaseEncodeGetDto>>> SearchCases(string filter, Guid subOrgId);


    }
}
