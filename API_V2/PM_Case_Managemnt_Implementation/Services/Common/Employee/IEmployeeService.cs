using PM_Case_Managemnt_Implementation.DTOS.Common;


namespace PM_Case_Managemnt_Implementation.Services.Common
{
    public interface IEmployeeService
    {

        public Task<int> CreateEmployee(EmployeeDto employee);
        public Task<int> UpdateEmployee(EmployeeDto employee);
        public Task<List<EmployeeDto>> GetEmployees(Guid subOrgId);
        public Task<EmployeeDto> GetEmployeesById(Guid employeeId);
        public Task<List<SelectListDto>> GetEmployeesNoUserSelectList(Guid subOrgId);

        public Task<List<SelectListDto>> GetEmployeesSelectList(Guid subOrgId);

        public Task<List<SelectListDto>> GetEmployeeByStrucutreSelectList(Guid StructureId);




    }
}
