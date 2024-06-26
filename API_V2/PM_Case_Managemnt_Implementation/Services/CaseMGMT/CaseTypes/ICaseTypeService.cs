using PM_Case_Managemnt_Implementation.DTOS.CaseDto;
using PM_Case_Managemnt_Implementation.DTOS.Common;

namespace PM_Case_Managemnt_Implementation.Services.CaseService.CaseTypes
{
    public interface ICaseTypeService
    {
        public Task Add(CaseTypePostDto caseTypeDto);
        public Task<List<CaseTypeGetDto>> GetAll(Guid subOrgId);
        public Task<List<SelectListDto>> GetAllByCaseForm(string caseForm, Guid subOrgId);
        public Task<List<SelectListDto>> GetAllSelectList(Guid subOrgId);

        public Task<List<SelectListDto>> GetFileSettigs(Guid caseTypeId);
        public Task UpdateCaseType(CaseTypePostDto caseTypeDto);
        public Task DeleteCaseType(Guid caseTypeId);
        public Task<List<SelectListDto>> GetChildCases(Guid caseTypeId);
        public int GetChildOrder(Guid caseTypeId);

        public Task<List<CaseTypeGetDto>> GetCaseTypeChildren(Guid caseTypeId);
    }
}
