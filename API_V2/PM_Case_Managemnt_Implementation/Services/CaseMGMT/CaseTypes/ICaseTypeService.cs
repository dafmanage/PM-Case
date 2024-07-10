using PM_Case_Managemnt_Implementation.DTOS.CaseDto;
using PM_Case_Managemnt_Implementation.DTOS.Common;
using PM_Case_Managemnt_Implementation.Helpers.Response;

namespace PM_Case_Managemnt_Implementation.Services.CaseService.CaseTypes
{
    public interface ICaseTypeService
    {
        public Task<ResponseMessage<int>> Add(CaseTypePostDto caseTypeDto);
        public Task<ResponseMessage<List<CaseTypeGetDto>>> GetAll(Guid subOrgId);
        public Task<ResponseMessage<List<SelectListDto>>> GetAllByCaseForm(string caseForm, Guid subOrgId);
        public Task<ResponseMessage<List<SelectListDto>>> GetAllSelectList(Guid subOrgId);
        public Task<ResponseMessage<List<SelectListDto>>> GetFileSettigs(Guid caseTypeId);
        public Task<ResponseMessage<int>> UpdateCaseType(CaseTypePostDto caseTypeDto);
        public Task<ResponseMessage<int>> DeleteCaseType(Guid caseTypeId);
        public Task<ResponseMessage<List<SelectListDto>>> GetChildCases(Guid caseTypeId);
        public Task<ResponseMessage<int>> GetChildOrder(Guid caseTypeId);
        public Task<ResponseMessage<List<CaseTypeGetDto>>> GetCaseTypeChildren(Guid caseTypeId);
    }
}
