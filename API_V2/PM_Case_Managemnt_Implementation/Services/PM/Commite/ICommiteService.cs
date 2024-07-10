using PM_Case_Managemnt_Implementation.DTOS.Common;
using PM_Case_Managemnt_Implementation.DTOS.PM;
using PM_Case_Managemnt_Implementation.Helpers.Response;

namespace PM_Case_Managemnt_Implementation.Services.PM.Commite
{
    public interface ICommiteService
    {
        public Task<ResponseMessage<List<CommiteListDto>>> GetCommiteLists(Guid subOrgId);
        public Task<ResponseMessage<int>> AddCommite(AddCommiteDto addCommiteDto);
        public Task<ResponseMessage<int>> UpdateCommite(UpdateCommiteDto updateCommite);
        public Task<ResponseMessage<List<SelectListDto>>> GetNotIncludedEmployees(Guid CommiteId, Guid subOrgId);

        public Task<ResponseMessage<int>> AddEmployeestoCommitte(CommiteEmployeesdto commiteEmployeesdto);

        public Task<ResponseMessage<int>> RemoveEmployeestoCommitte(CommiteEmployeesdto commiteEmployeesdto);
        public Task<ResponseMessage<List<SelectListDto>>> GetSelectListCommittee(Guid subOrgId);
        public Task<ResponseMessage<List<SelectListDto>>> GetCommiteeEmployees(Guid comitteId);
    }
}
