using PM_Case_Managemnt_Implementation.DTOS.Common;
using PM_Case_Managemnt_Implementation.Helpers.Response;

namespace PM_Case_Managemnt_Implementation.Services.Common
{
    public interface IOrgStructureService
    {

        public Task<ResponseMessage<int>> CreateOrganizationalStructure(OrgStructureDto orgStructure);

        public Task<ResponseMessage<int>> UpdateOrganizationalStructure(OrgStructureDto organizationProfile);
        public Task<ResponseMessage<List<OrgStructureDto>>> GetOrganizationStructures(Guid SubOrgId, Guid? BranchId);

        public Task<ResponseMessage<List<SelectListDto>>> getParentStrucctureSelectList(Guid branchId);

        public Task<ResponseMessage<List<DiagramDto>>> getDIagram(Guid? BranchId);


    }
}
