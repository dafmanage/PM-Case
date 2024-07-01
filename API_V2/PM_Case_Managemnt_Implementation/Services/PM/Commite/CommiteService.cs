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
            return await (from t in _dBContext.Commitees.Include(x => x.Employees).Where(y => y.SubsidiaryOrganizationId == subOrgId).AsNoTracking()
                          select new CommiteListDto
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

        public async Task<List<SelectListDto>> GetNotIncludedEmployees(Guid CommiteId, Guid subOrgId)
        {
            var EmployeeSelectList = await (from e in _dBContext.Employees.Include(x => x.OrganizationalStructure).Where(x => x.OrganizationalStructure.SubsidiaryOrganizationId == subOrgId)

                                            where !(_dBContext.CommiteEmployees.Where(x => x.CommiteeId.Equals(CommiteId)).Select(x => x.EmployeeId).Contains(e.Id))
                                            select new SelectListDto
                                            {
                                                Id = e.Id,
                                                Name = e.FullName + " ( " + e.OrganizationalStructure.StructureName + " ) "

                                            }).ToListAsync();

            return EmployeeSelectList;
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

        public async Task<int> AddEmployeestoCommitte(CommiteEmployeesdto commiteEmployeesdto)
        {

            foreach (var c in commiteEmployeesdto.EmployeeList)
            {

                var committeeemployee = new CommitesEmployees
                {
                    Id = Guid.NewGuid(),
                    CommiteeId = commiteEmployeesdto.CommiteeId,
                    EmployeeId = c,
                    CreatedAt = DateTime.Now,
                    CreatedBy = commiteEmployeesdto.CreatedBy

                };

                await _dBContext.AddAsync(committeeemployee);
                await _dBContext.SaveChangesAsync();

            }

            return 1;

        }
        public async Task<int> RemoveEmployeestoCommitte(CommiteEmployeesdto commiteEmployeesdto)
        {

            foreach (var c in commiteEmployeesdto.EmployeeList)
            {

                var emp = _dBContext.CommiteEmployees.Where(x => x.CommiteeId == commiteEmployeesdto.CommiteeId && x.EmployeeId == c);

                _dBContext.RemoveRange(emp);
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
