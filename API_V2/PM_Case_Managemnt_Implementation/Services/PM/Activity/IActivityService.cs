using PM_Case_Managemnt_Implementation.DTOS.Common;
using PM_Case_Managemnt_Implementation.DTOS.PM;
using PM_Case_Managemnt_Implementation.Helpers.Response;


namespace PM_Case_Managemnt_Implementation.Services.PM.Activity
{
    public interface IActivityService
    {
        public Task<int> AddActivityDetails(ActivityDetailDto activityDetail);
        public Task<int> AddSubActivity(SubActivityDetailDto subActivity);

        public Task<int> AddTargetActivities(ActivityTargetDivisionDto targetDivisions);

        public Task<int> AddProgress(AddProgressActivityDto activityProgress);

        public Task<List<ProgressViewDto>> ViewProgress(Guid actId);


        public Task<List<ActivityViewDto>> GetAssignedActivity(Guid employeeId);

        public Task<int> GetAssignedActivityNumber(Guid employeeId);


        public Task<List<ActivityViewDto>> GetActivtiesForApproval(Guid employeeId);


        public Task<int> ApproveProgress(ApprovalProgressDto approvalProgressDto);


        public Task<List<ActivityAttachmentDto>> getAttachemnts(Guid taskId);
        public Task<ActivityViewDto> getActivityById(Guid actId);


        public Task<List<SelectListDto>> GetEmployeesInBranch(Guid branchId);

        public Task<ReponseMessage> AssignEmployees(ActivityEmployees activityEmployee);

        public Task<ResponseMessage<int>> UpdateActivityDetails(SubActivityDetailDto activityDetail);
        public Task<ResponseMessage<int>> DeleteActivity(Guid activityId, Guid taskId);


    }
}
