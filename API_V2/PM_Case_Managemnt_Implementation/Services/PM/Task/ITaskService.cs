using PM_Case_Managemnt_Implementation.DTOS.Common;
using PM_Case_Managemnt_Implementation.DTOS.PM;
using PM_Case_Managemnt_Implementation.Helpers;
namespace PM_Case_Managemnt_Implementation.Services.PM
{
    public interface ITaskService
    {
        public Task<int> CreateTask(TaskDto task);

        public Task<TaskViewDto> GetSingleTask(Guid taskId);

        public Task<int> AddTaskMemebers(TaskMembersDto taskMembers);

        public Task<int> AddTaskMemo(TaskMemoRequestDto taskMemo);

        public Task<List<SelectListDto>> GetEmployeesNoTaskMembersSelectList(Guid taskId, Guid subOrgId);

        public Task<List<SelectListDto>> GetTasksSelectList(Guid PlanId);

        public Task<List<SelectListDto>> GetActivitieParentsSelectList(Guid TaskId);

        public Task<List<SelectListDto>> GetActivitiesSelectList(Guid? planId, Guid? taskId, Guid? actParentId);

        public Task<List<ActivityViewDto>> GetSingleActivityParent(Guid actParentId);

        public Task<ResponseMessage> UpdateTask(TaskDto updateTask);

        public Task<ResponseMessage> DeleteTask(Guid taskId);
    }
}
