using Microsoft.EntityFrameworkCore;
using PM_Case_Managemnt_Implementation.DTOS.Common;
using PM_Case_Managemnt_Implementation.DTOS.PM;
using PM_Case_Managemnt_Infrustructure.Data;
using PM_Case_Managemnt_Infrustructure.Models.Common;
using PM_Case_Managemnt_Infrustructure.Models.PM;
using System.Collections.Immutable;
using PM_Case_Managemnt_Implementation.Helpers.Logger;
using PM_Case_Managemnt_Implementation.Helpers.Response;

namespace PM_Case_Managemnt_Implementation.Services.PM.Commite
{
    public class CommiteService : ICommiteService
    {
        private readonly ApplicationDbContext _dBContext;
        private readonly ILoggerManagerService _logger;
        public CommiteService(ApplicationDbContext context, ILoggerManagerService logger)
        {
            _dBContext = context;
            _logger = logger;
        }

        public async Task<ResponseMessage<int>> AddCommite(AddCommiteDto addCommiteDto)
        {
            var response = new ResponseMessage<int>();
            var Commite = new Commitees
            {
                Id = Guid.NewGuid(),
                CommiteeName = addCommiteDto.Name,
                CreatedAt = DateTime.Now,
                CreatedBy = addCommiteDto.CreatedBy,
                Remark = addCommiteDto.Remark,
                RowStatus = RowStatus.Active,
                SubsidiaryOrganizationId = addCommiteDto.SubsidiaryOrganizationId
            };

            await _dBContext.AddAsync(Commite);
            await _dBContext.SaveChangesAsync();
            response.Message = "Operation Successful.";
            response.Success = true;
            response.Data = 1;
            _logger.LogCreate("CommiteService", addCommiteDto.CreatedBy.ToString(), "Commite added Successfully");
            return response;
        }

        public async Task<ResponseMessage<List<CommiteListDto>>> GetCommiteLists(Guid subOrgId)
        {
            var response = new ResponseMessage<List<CommiteListDto>>();
            
            List<CommiteListDto> result =  await (from t in _dBContext.Commitees.Include(x=>x.Employees).Where(y => y.SubsidiaryOrganizationId == subOrgId).AsNoTracking()
                select new CommiteListDto
                {
                    Id = t.Id,
                    Name= t.CommiteeName,
                    NoOfEmployees = t.Employees.Count(),
                    EmployeeList = t.Employees.Select(e => new SelectListDto
                    {
                        Name = e.Employee.FullName,
                        CommiteeStatus = e.CommiteeEmployeeStatus.ToString(),
                        Id = e.Employee.Id,
                    }).ToList(),
                    Remark = t.Remark
                }).ToListAsync();

            response.Message = "Operation Successful.";
            response.Success = true;
            response.Data = result;

            return response;


        }

        public async Task<ResponseMessage<List<SelectListDto>>> GetNotIncludedEmployees(Guid CommiteId, Guid subOrgId)
        {
            var response = new ResponseMessage<List<SelectListDto>>();
            var notIncludedEmployees = await _dBContext.Employees
                .Include(e => e.OrganizationalStructure)
                .Where(e => e.OrganizationalStructure.SubsidiaryOrganizationId == subOrgId)
                .Where(e => !_dBContext.CommiteEmployees
                    .Where(ce => ce.CommiteeId == commiteId)
                    .Select(ce => ce.EmployeeId)
                    .Contains(e.Id))
                .Select(e => new SelectListDto
                {
                    Id = e.Id,
                    Name = $"{e.FullName} ({e.OrganizationalStructure.StructureName})"
                })
                .ToListAsync();

                                            

            response.Message = "Operation Successful.";
            response.Success = true;
            response.Data = notIncludedEmployees;

            return response;
        }

        public async Task<ResponseMessage<int>> UpdateCommite(UpdateCommiteDto updateCommite)
        {
            var response = new ResponseMessage<int>();
            
            var currentCommite = await _dBContext.Commitees.FirstOrDefaultAsync(x => x.Id.Equals(updateCommite.Id));
            if (currentCommite != null)
            {
                currentCommite.CommiteeName = updateCommite.Name;
                currentCommite.Remark = updateCommite.Remark;
                currentCommite.RowStatus = updateCommite.RowStatus;
                await _dBContext.SaveChangesAsync();

                response.Message = "Operation Successful.";
                response.Success = true;
                response.Data = 1;

                return response;
            }
            response.Message = "Operation Successful.";
            response.Success = true;
            response.Data = 0;
            _logger.LogUpdate("CommiteService", updateCommite.CreatedBy.ToString(), "Commite Updated Successfully");
            return response;
        }

        public async Task<ResponseMessage<int>> AddEmployeestoCommitte(CommiteEmployeesdto commiteEmployeesdto)
        {
            var response = new ResponseMessage<int>();
            var committeeEmployees = commiteEmployeesDto.EmployeeList.Select(employeeId => new CommitesEmployees
            {
                Id = Guid.NewGuid(),
                CommiteeId = commiteEmployeesDto.CommiteeId,
                EmployeeId = employeeId,
                CreatedAt = DateTime.Now,
                CreatedBy = commiteEmployeesDto.CreatedBy
            }).ToList();

            await _dBContext.AddRangeAsync(committeeEmployees);
            await _dBContext.SaveChangesAsync();

            

            response.Message = "Operation Successful.";
            response.Success = true;
            response.Data = 1;
            _logger.LogCreate("CommiteService", commiteEmployeesdto.CreatedBy.ToString(), "Employees added to committe Successfully");
            return response;

        }
        public async Task<ResponseMessage<int>> RemoveEmployeesFromCommitte(CommiteEmployeesdto commiteEmployeesdto)
        {
            var response = new ResponseMessage<int>();
            var employeesToRemove = await _dBContext.CommiteEmployees
                .Where(x => x.CommiteeId == commiteEmployeesDto.CommiteeId && commiteEmployeesDto.EmployeeList.Contains(x.EmployeeId))
                .ToListAsync();

            if (employeesToRemove.Count != 0)
            {
                _dBContext.RemoveRange(employeesToRemove);
                await _dBContext.SaveChangesAsync();
            }

            response.Message = "Operation Successful.";
            response.Success = true;
            response.Data = 1;
            _logger.LogUpdate("CommiteService", commiteEmployeesdto.CreatedBy.ToString(), "removed Employees from committe Successfully");
            return response;

        }

        public async Task<ResponseMessage<List<SelectListDto>>> GetSelectListCommittee(Guid subOrgId)
        {

            var response = new ResponseMessage<List<SelectListDto>>();

            List<SelectListDto> result =  await (from c in _dBContext.Commitees.Where(v => v.SubsidiaryOrganizationId== subOrgId)
                select new SelectListDto
                {
                    Id = c.Id,
                    Name= c.CommiteeName
                }).ToListAsync();
            response.Message = "Operation Successful.";
            response.Success = true;
            response.Data = result;

            return response;
        }

        public async Task<ResponseMessage<List<SelectListDto>>> GetCommiteeEmployees(Guid comitteId)
        {
            var response = new ResponseMessage<List<SelectListDto>>();

            List<SelectListDto> result =  await _dBContext.CommiteEmployees.Include(x=>x.Employee).Where(x=>x.CommiteeId==comitteId).Select(x=> new SelectListDto
            {
                Id = x.Id,
                Name= x.Employee.FullName,
                CommiteeStatus = x.CommiteeEmployeeStatus.ToString(),
                
            }).ToListAsync();
            
            response.Message = "Operation Successful.";
            response.Success = true;
            response.Data = result;

            return response;
        }
    }
}
