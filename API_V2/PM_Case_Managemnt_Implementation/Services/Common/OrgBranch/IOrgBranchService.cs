using PM_Case_Managemnt_Implementation.DTOS.Common;

namespace PM_Case_Managemnt_Implementation.Services.Common
{
    public interface IOrgBranchService
    {

        public Task<int> CreateOrganizationalBranch(OrgBranchDto organizationBranch);

        public Task<int> UpdateOrganizationBranch(OrgBranchDto organizationBranch);
        public Task<List<OrgStructureDto>> GetOrganizationBranches(Guid SubOrgId);

        public Task<List<SelectListDto>> getBranchSelectList(Guid SubOrgId);
    }
}
