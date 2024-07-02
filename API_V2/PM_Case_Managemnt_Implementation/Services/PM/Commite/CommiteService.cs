using Microsoft.EntityFrameworkCore;
using PM_Case_Managemnt_Implementation.DTOS.Common;
using PM_Case_Managemnt_Implementation.DTOS.PM;
using PM_Case_Managemnt_Infrustructure.Data;
using PM_Case_Managemnt_Infrustructure.Models.Common;
using PM_Case_Managemnt_Infrustructure.Models.PM;
using System.Collections.Immutable;

namespace PM_Case_Managemnt_Implementation.Services.PM.Commite
{
    public class CommiteService : ICommiteService
    {
        private readonly ApplicationDbContext _dBContext;
        public CommiteService(ApplicationDbContext context)
        {
            _dBContext = context;
        }

        public async Task<int> AddCommite(AddCommiteDto addCommiteDto)
        {
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

            return 1;
        }

        public async Task<List<CommiteListDto>> GetCommiteLists(Guid subOrgId)
        {
            return await _dBContext.Commitees
                .Include(x => x.Employees)
                .Where(y => y.SubsidiaryOrganizationId == subOrgId)
                .AsNoTracking()
                .Select(t => new CommiteListDto
                {
                    Id = t.Id,
                    Name = t.CommiteeName,
                    NoOfEmployees = t.Employees.Count(),
                    EmployeeList = t.Employees.Select(e => new SelectListDto
                    {
                        Name = e.Employee.FullName,
                        CommiteeStatus = e.CommiteeEmployeeStatus.ToString(),
                        Id = e.Employee.Id,
                    }).ToList(),
                    Remark = t.Remark
                }).ToListAsync();
        }

        public async Task<List<SelectListDto>> GetNotIncludedEmployees(Guid commiteId, Guid subOrgId)
        {
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

            return notIncludedEmployees;
        }

        public async Task<int> UpdateCommite(UpdateCommiteDto updateCommite)
        {
            var currentCommite = await _dBContext.Commitees.FirstOrDefaultAsync(x => x.Id.Equals(updateCommite.Id));
            if (currentCommite != null)
            {
                currentCommite.CommiteeName = updateCommite.Name;
                currentCommite.Remark = updateCommite.Remark;
                currentCommite.RowStatus = updateCommite.RowStatus;
                await _dBContext.SaveChangesAsync();

                return 1;
            }
            return 0;
        }

        public async Task<int> AddEmployeesToCommittee(CommiteEmployeesdto commiteEmployeesDto)
        {
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

            return 1;
        }

        public async Task<int> RemoveEmployeesFromCommittee(CommiteEmployeesdto commiteEmployeesDto)
        {
            var employeesToRemove = await _dBContext.CommiteEmployees
                .Where(x => x.CommiteeId == commiteEmployeesDto.CommiteeId && commiteEmployeesDto.EmployeeList.Contains(x.EmployeeId))
                .ToListAsync();

            if (employeesToRemove.Count != 0)
            {
                _dBContext.RemoveRange(employeesToRemove);
                await _dBContext.SaveChangesAsync();
            }

            return 1;
        }

        public async Task<List<SelectListDto>> GetSelectListCommittee(Guid subOrgId)
        {

            return await (from c in _dBContext.Commitees.Where(v => v.SubsidiaryOrganizationId == subOrgId)
                          select new SelectListDto
                          {
                              Id = c.Id,
                              Name = c.CommiteeName
                          }).ToListAsync();
        }
        public async Task<List<SelectListDto>> GetCommiteeEmployees(Guid comitteId)
        {
            return await _dBContext.CommiteEmployees.Include(x => x.Employee).Where(x => x.CommiteeId == comitteId).Select(x => new SelectListDto
            {
                Id = x.Id,
                Name = x.Employee.FullName,
                CommiteeStatus = x.CommiteeEmployeeStatus.ToString(),

            }).ToListAsync();
        }
    }
}
