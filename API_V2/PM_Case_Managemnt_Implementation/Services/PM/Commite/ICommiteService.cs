using PM_Case_Managemnt_Implementation.DTOS.Common;
using PM_Case_Managemnt_Implementation.DTOS.PM;

namespace PM_Case_Managemnt_Implementation.Services.PM.Commite
{
    public interface ICommiteService
    {
        public Task<List<CommiteListDto>> GetCommiteLists(Guid subOrgId);
        public Task<int> AddCommite(AddCommiteDto addCommiteDto);
        public Task<int> UpdateCommite(UpdateCommiteDto updateCommite);
        public Task<List<SelectListDto>> GetNotIncludedEmployees(Guid CommiteId, Guid subOrgId);

        public Task<int> AddEmployeesToCommittee(CommiteEmployeesdto commiteEmployeesdto);

        public Task<int> RemoveEmployeesFromCommittee(CommiteEmployeesdto commiteEmployeesdto);
        public Task<List<SelectListDto>> GetSelectListCommittee(Guid subOrgId);
        public Task<List<SelectListDto>> GetCommiteeEmployees(Guid comitteId);
    }
}
