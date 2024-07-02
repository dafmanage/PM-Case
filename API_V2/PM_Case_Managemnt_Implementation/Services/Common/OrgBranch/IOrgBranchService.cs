using PM_Case_Managemnt_Implementation.DTOS.Common;
using PM_Case_Managemnt_Implementation.Helpers.Response;

namespace PM_Case_Managemnt_Implementation.Services.Common
{
    public interface IOrgBranchService
    {

        public Task<ResponseMessage<int>> CreateOrganizationalBranch(OrgBranchDto organizationBranch);

        public Task<ResponseMessage<int>> UpdateOrganizationBranch(OrgBranchDto organizationBranch);
        public Task<ResponseMessage<List<OrgStructureDto>>> GetOrganizationBranches(Guid SubOrgId);

        public Task<ResponseMessage<List<SelectListDto>>> getBranchSelectList(Guid SubOrgId);
    }
}
