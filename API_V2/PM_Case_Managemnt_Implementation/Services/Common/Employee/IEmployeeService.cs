using PM_Case_Managemnt_Implementation.DTOS.Common;
using PM_Case_Managemnt_Implementation.Helpers.Response;


namespace PM_Case_Managemnt_Implementation.Services.Common
{
    public interface IEmployeeService
    {

        public Task<ResponseMessage<int>> CreateEmployee(EmployeeDto employee);
        public Task<ResponseMessage<int>> UpdateEmployee(EmployeeDto employee);
        public Task<ResponseMessage<List<EmployeeDto>>> GetEmployees(Guid subOrgId);
        public Task<ResponseMessage<EmployeeDto>> GetEmployeesById(Guid employeeId);
        public Task<ResponseMessage<List<SelectListDto>>> GetEmployeesNoUserSelectList(Guid subOrgId);
      
        public Task<ResponseMessage<List<SelectListDto>>> GetEmployeesSelectList(Guid subOrgId);

        public Task<ResponseMessage<List<SelectListDto>>> GetEmployeeByStrucutreSelectList(Guid StructureId);





    }
}
