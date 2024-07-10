using PM_Case_Managemnt_Implementation.DTOS.Common;
using PM_Case_Managemnt_Implementation.DTOS.PM;
using PM_Case_Managemnt_Implementation.Helpers;
using PM_Case_Managemnt_Implementation.Helpers.Response;

namespace PM_Case_Managemnt_Implementation.Services.PM
{
    public interface ITaskService
    {

        public Task<ResponseMessage<int>> CreateTask(TaskDto task);

        public Task<ResponseMessage<TaskVIewDto>> GetSingleTask(Guid taskId);


        public Task<ResponseMessage<int>> AddTaskMemebers(TaskMembersDto taskMembers);

        public Task<ResponseMessage<int>> AddTaskMemo(TaskMemoRequestDto taskMemo);


        public Task<ResponseMessage<List<SelectListDto>>> GetEmployeesNoTaskMembersSelectList(Guid taskId, Guid subOrgId);


        public Task<ResponseMessage<List<SelectListDto>>> GetTasksSelectList(Guid PlanId);

        public Task<ResponseMessage<List<SelectListDto>>> GetActivitieParentsSelectList(Guid TaskId);
        public Task<ResponseMessage<List<SelectListDto>>> GetActivitiesSelectList(Guid? planId, Guid? taskId, Guid? actParentId);
        public Task<ResponseMessage<List<ActivityViewDto>>> GetSingleActivityParent(Guid actParentId);
        public Task<ResponseMessage> UpdateTask(TaskDto updateTask);
        public Task<ResponseMessage> DeleteTask(Guid taskId);



    }
}
