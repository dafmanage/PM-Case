using System.Net;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PM_Case_Managemnt_Implementation.DTOS.Common;
using PM_Case_Managemnt_Implementation.Helpers.Response;
using PM_Case_Managemnt_Infrustructure.Data;
using PM_Case_Managemnt_Infrustructure.Models.Auth;
using PM_Case_Managemnt_Infrustructure.Models.CaseModel;
using PM_Case_Managemnt_Infrustructure.Models.Common;



namespace PM_Case_Managemnt_Implementation.Services.Common
{
    public class EmployeeService : IEmployeeService
    {

        private readonly ApplicationDbContext _dBContext;
        private readonly UserManager<ApplicationUser> _userManager;
        public EmployeeService(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _dBContext = context;
            _userManager = userManager;
        }

        public async Task<ResponseMessage<int>> CreateEmployee(EmployeeDto employee)
        {

            var response = new ResponseMessage<int>();
            try
            {
                if (employee.Id == null || employee.Id == Guid.Empty)
                {
                    employee.Id = Guid.NewGuid();

                }

                Employee employee1 = new Employee
                {
                    Id = (Guid)employee.Id,
                    CreatedAt = DateTime.Now,
                    Photo = employee.Photo,
                    FullName = employee.FullName,
                    Title = employee.Title,
                    PhoneNumber = employee.PhoneNumber,
                    Gender = Enum.Parse<Gender>(employee.Gender),
                    Remark = employee.Remark,
                    OrganizationalStructureId = Guid.Parse(employee.StructureId),
                    Position = Enum.Parse<Position>(employee.Position),
                    MobileUsersMacaddress = "1234",
                    UserName = employee.FullName.Split(' ')[0],
                    Password = "123456"

                };

                await _dBContext.Employees.AddAsync(employee1);
                await _dBContext.SaveChangesAsync();
                response.Message = "OPeration Successfull";
                response.Success = true;
                response.Data = 1;
                return response;
            }
            catch (Exception ex)
            {
                response.Message = $"{ex.Message}";
                response.Success = false;
                response.Data = -1;
                response.ErrorCode = HttpStatusCode.InternalServerError.ToString();
                return response;
            }

        }

        public async Task<ResponseMessage<List<SelectListDto>>> GetEmployeesNoUserSelectList(Guid subOrgId)
        {
            var response = new ResponseMessage<List<SelectListDto>>();
            var emp = _userManager.Users.Where(j => j.SubsidiaryOrganizationId == subOrgId).Select(x => x.EmployeesId).ToList();


            //var EmployeeSelectList = await (from e in _dBContext.Employees
            //                                where e.OrganizationalStructure.SubsidiaryOrganizationId == subOrgId && !emp.Contains(e.Id)
            //                                select new SelectListDto
            //                                {
            //                                    Id = e.Id,
            //                                    Name = e.FullName

            //                                }).ToListAsync();
            var EmployeeSelectList = await _dBContext.Employees.Where(e =>
                                                e.OrganizationalStructure.SubsidiaryOrganizationId == subOrgId && !emp.Contains(e.Id))
                                            .Select(e => new SelectListDto
                                            {
                                                Id = e.Id,
                                                Name = e.FullName
                                            })
                                            .ToListAsync();

            response.Message = "Operation Successful.";
            response.Data = EmployeeSelectList;
            response.Success = true;

            return response;    

        }



        public async Task<ResponseMessage<List<EmployeeDto>>> GetEmployees(Guid subOrgId)
        {

            var response = new ResponseMessage<List<EmployeeDto>>();
            
            var k = await (from e in _dBContext.Employees.Include(x => x.OrganizationalStructure).Where(x => x.OrganizationalStructure.SubsidiaryOrganizationId == subOrgId)
                           join x in _dBContext.OrganizationalStructures on e.OrganizationalStructure.OrganizationBranchId equals x.Id

                           select new EmployeeDto
                           {
                               Id = e.Id,
                               Photo = e.Photo,
                               Title = e.Title,
                               FullName = e.FullName,
                               Gender = e.Gender.ToString(),
                               PhoneNumber = e.PhoneNumber,
                               Position = e.Position.ToString(),
                               StructureName = e.OrganizationalStructure.StructureName,
                               BranchId = e.OrganizationalStructure.OrganizationBranchId.ToString(),
                               StructureId = e.OrganizationalStructureId.ToString(),
                               Remark = e.Remark,
                               BranchName = x.StructureName,
                               RowStatus = x.RowStatus == RowStatus.Active ? 0 : 1

                           }).ToListAsync();

            response.Message = "Operation Successful";
            response.Data = k;
            response.Success = true;
            return response;
        }

        public async Task<ResponseMessage<EmployeeDto>> GetEmployeesById(Guid employeeId)
        {
            var response = new ResponseMessage<EmployeeDto>();

            try
            {
                var k = await (from e in _dBContext.Employees.Include(x => x.OrganizationalStructure.ParentStructure).Where(x => x.Id == employeeId)

                               select new EmployeeDto
                               {
                                   Id = e.Id,
                                   Photo = e.Photo,
                                   Title = e.Title,
                                   FullName = e.FullName,
                                   Gender = e.Gender.ToString(),
                                   PhoneNumber = e.PhoneNumber,
                                   Position = e.Position.ToString(),
                                   StructureName = e.OrganizationalStructure.StructureName,
                                   BranchId = e.OrganizationalStructure.OrganizationBranchId.ToString(),
                                   StructureId = e.OrganizationalStructureId.ToString(),
                                   Remark = e.Remark

                               }).FirstOrDefaultAsync();
                if (k == null)
                {
                    response.Message = "Could not get Employee.";
                    response.Success = false;
                    response.Data = null;
                    response.ErrorCode = HttpStatusCode.NotFound.ToString();

                    return response;
                }
                k.BranchId = _dBContext.OrganizationalStructures.Find(Guid.Parse(k.BranchId)) != null ? _dBContext.OrganizationalStructures.Find(Guid.Parse(k.BranchId)).StructureName : "";
                response.Message = "Operation Successful";
                response.Success = true;
                response.Data = k;
                
                return response;
            }
            catch (Exception e)
            {
                response.Message = $"{e.Message}";
                response.Success = false;
                response.Data = null;
                response.ErrorCode = HttpStatusCode.InternalServerError.ToString();

                return response;
            }


        }

        public async Task<ResponseMessage<List<SelectListDto>>> GetEmployeesSelectList(Guid subOrgId)
        {
            var response = new ResponseMessage<List<SelectListDto>>();
            
            var EmployeeSelectList = await (from e in _dBContext.Employees.Where(x => x.OrganizationalStructure.SubsidiaryOrganizationId == subOrgId && x.RowStatus == RowStatus.Active)

                                            select new SelectListDto
                                            {
                                                Id = e.Id,
                                                Name = e.FullName

                                            }).ToListAsync();

            response.Message = "Operation Successful";
            response.Data = EmployeeSelectList;
            response.Success = true;
            
            return response;



        }





        public async Task<ResponseMessage<int>> UpdateEmployee(EmployeeDto employeeDto)
        {

            var response = new ResponseMessage<int>();


            var orgEmployee = _dBContext.Employees.Find(employeeDto.Id);

            if (orgEmployee == null)
            {
                
                response.Message = "Could not get Employee.";
                response.Success = false;
                response.Data = -1;
                response.ErrorCode = HttpStatusCode.NotFound.ToString();

                return response;
            }
            orgEmployee.Photo = employeeDto.Photo;
            orgEmployee.Title = employeeDto.Title;
            orgEmployee.FullName = employeeDto.FullName;
            orgEmployee.Gender = Enum.Parse<Gender>(employeeDto.Gender);
            orgEmployee.PhoneNumber = employeeDto.PhoneNumber;
            orgEmployee.Remark = employeeDto.Remark;
            orgEmployee.Position = Enum.Parse<Position>(employeeDto.Position);
            orgEmployee.OrganizationalStructureId = Guid.Parse(employeeDto.StructureId);
            orgEmployee.RowStatus = employeeDto.RowStatus == 0 ? RowStatus.Active : RowStatus.InActive;


            _dBContext.Entry(orgEmployee).State = EntityState.Modified;
            await _dBContext.SaveChangesAsync();
            response.Message = "Operation Successful.";
            response.Data = 1;
            response.Success = true;
            
            return response;

        }

        public async Task<ResponseMessage<List<SelectListDto>>> GetEmployeeByStrucutreSelectList(Guid StructureId)
        {

            var response = new ResponseMessage<List<SelectListDto>>();

            var affairs = _dBContext.Cases.Where(x => x.AffairStatus != AffairStatus.Completed && x.AffairStatus != AffairStatus.Encoded).ToList();




            List<SelectListDto> employees = await (from e in _dBContext.Employees.Where(x => x.OrganizationalStructureId == StructureId)

                                                   select new SelectListDto
                                                   {
                                                       Id = e.Id,
                                                       Name = $"{e.FullName} ( {e.Position} )"
                                                   }).ToListAsync();
            foreach (var emp in employees)
            {
                int workLoad = 0;
                foreach (var affair in affairs)
                {
                    var maxChild = _dBContext.CaseHistories.Where(x => x.CaseId == affair.Id && x.ReciverType == ReciverType.Orginal).OrderByDescending(z => z.childOrder).FirstOrDefault().childOrder;
                    workLoad += _dBContext.CaseHistories.Count(y => y.ToEmployeeId == emp.Id && (y.childOrder == maxChild) && y.CaseId == affair.Id && y.ReciverType == ReciverType.Orginal);
                }
                emp.Name += " ( " + workLoad.ToString() + " Total Tasks )";

            }






            response.Message = "Operation Successdul.";
            response.Data = employees;
            response.Success = true;

            return response;

        }



    }
}
