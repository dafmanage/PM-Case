
using Microsoft.EntityFrameworkCore;
using PM_Case_Managemnt_Implementation.DTOS.Common;
using PM_Case_Managemnt_Infrustructure.Data;
using PM_Case_Managemnt_Infrustructure.Models.Common;

namespace PM_Case_Managemnt_Implementation.Services.Common
{
    public class UnitOfMeasurmentService : IUnitOfMeasurmentService
    {


        private readonly ApplicationDbContext _dBContext;
        public UnitOfMeasurmentService(ApplicationDbContext context)
        {
            _dBContext = context;
        }

        public async Task<int> CreateUnitOfMeasurment(UnitOfMeasurmentDto UnitOfMeasurment)
        {


            var unitOfMeasurment = new UnitOfMeasurment
            {
                Id = Guid.NewGuid(),
                Name = UnitOfMeasurment.Name,
                LocalName = UnitOfMeasurment.LocalName,
                Type = UnitOfMeasurment.Type == 0 ? MeasurmentType.percent : MeasurmentType.number,
                CreatedAt = DateTime.Now,
                Remark = UnitOfMeasurment.Remark,
                SubsidiaryOrganizationId = UnitOfMeasurment.SubsidiaryOrganizationId
            };


            await _dBContext.AddAsync(unitOfMeasurment);
            await _dBContext.SaveChangesAsync();

            return 1;

        }
        public async Task<List<PM_Case_Managemnt_Infrustructure.Models.Common.UnitOfMeasurment>> GetUnitOfMeasurment(Guid subOrgId)
        {



            return await _dBContext.UnitOfMeasurment.Where(x => x.SubsidiaryOrganizationId == subOrgId).ToListAsync();
            //return k;
        }

        public async Task<List<SelectListDto>> getUnitOfMeasurmentSelectList(Guid subOrgId)
        {

            List<SelectListDto> list = await (from x in _dBContext.UnitOfMeasurment.Where(x => x.SubsidiaryOrganizationId == subOrgId)
                                              select new SelectListDto
                                              {
                                                  Id = x.Id,
                                                  Name = x.Name + " ( " + x.LocalName + " ) "

                                              }).ToListAsync();


            return list;
        }

        public async Task<int> UpdateUnitOfMeasurment(UnitOfMeasurmentDto unitOfMeasurmentDto)
        {

            var unitMeasurment = _dBContext.UnitOfMeasurment.Find(unitOfMeasurmentDto.Id);

            unitMeasurment.Name = unitOfMeasurmentDto.Name;
            unitMeasurment.LocalName = unitOfMeasurmentDto.LocalName;
            unitMeasurment.Type = unitOfMeasurmentDto.Type == 0 ? MeasurmentType.percent : MeasurmentType.number;
            unitMeasurment.Remark = unitOfMeasurmentDto.Remark;
            unitMeasurment.RowStatus = unitOfMeasurmentDto.RowStatus == 0 ? RowStatus.Active : RowStatus.InActive;

            _dBContext.Entry(unitMeasurment).State = EntityState.Modified;
            await _dBContext.SaveChangesAsync();
            return 1;

        }
    }
}
