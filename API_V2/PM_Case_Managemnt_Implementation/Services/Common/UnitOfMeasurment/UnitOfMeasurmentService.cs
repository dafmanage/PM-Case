
using System.Net;
using Microsoft.EntityFrameworkCore;
using PM_Case_Managemnt_Implementation.DTOS.Common;
using PM_Case_Managemnt_Implementation.Helpers.Logger;
using PM_Case_Managemnt_Implementation.Helpers.Response;
using PM_Case_Managemnt_Infrustructure.Data;
using PM_Case_Managemnt_Infrustructure.Models.Common;

namespace PM_Case_Managemnt_Implementation.Services.Common
{
    public class UnitOfMeasurmentService : IUnitOfMeasurmentService
    {


        private readonly ApplicationDbContext _dBContext;
        private readonly ILoggerManagerService _logger;
        public UnitOfMeasurmentService(ApplicationDbContext context, ILoggerManagerService logger)
        {
            _dBContext = context;
            _logger = logger;
        }

        public async Task<ResponseMessage<int>> CreateUnitOfMeasurment(UnitOfMeasurmentDto UnitOfMeasurment)
        {

            var response = new ResponseMessage<int>();
            var unitOfMeasurment = new UnitOfMeasurment
            {
                Id = Guid.NewGuid(),
                Name = UnitOfMeasurment.Name,
                LocalName = UnitOfMeasurment.LocalName,
                Type = UnitOfMeasurment.Type == 0 ? MeasurmentType.percent : MeasurmentType.number,
                CreatedAt = DateTime.Now,
                Remark= UnitOfMeasurment.Remark,
                SubsidiaryOrganizationId = UnitOfMeasurment.SubsidiaryOrganizationId
            };
            

            await _dBContext.AddAsync(unitOfMeasurment);
            await _dBContext.SaveChangesAsync();

            response.Message = "Operation Successful.";
            response.Data = 1;
            response.Success = true;
            _logger.LogCreate("unitOfMeasurmentService",UnitOfMeasurment.Id.ToString(), "Unit Of measurement created Successfully");
            return response;

        }
        public async Task<ResponseMessage<List<PM_Case_Managemnt_Infrustructure.Models.Common.UnitOfMeasurment>>> GetUnitOfMeasurment(Guid subOrgId)
        {


            var response = new ResponseMessage<List < PM_Case_Managemnt_Infrustructure.Models.Common.UnitOfMeasurment >> ();
            var result = await _dBContext.UnitOfMeasurment.Where(x => x.SubsidiaryOrganizationId == subOrgId).ToListAsync();
            //return k;
            response.Message = "Operation Successful.";
            response.Data = result;
            response.Success = true;
            
            return response;
        }

        public async Task<ResponseMessage<List<SelectListDto>>> getUnitOfMeasurmentSelectList(Guid subOrgId)
        {
            var response = new ResponseMessage<List<SelectListDto>>();
            
            List<SelectListDto> list = await (from x in _dBContext.UnitOfMeasurment.Where(x => x.SubsidiaryOrganizationId == subOrgId)
                select new SelectListDto
                {
                    Id = x.Id,
                    Name = x.Name + " ( "+x.LocalName+" ) "

                }).ToListAsync();


            response.Message = "Operation Successful.";
            response.Data = list;
            response.Success = true;
            
            return response;
        }

        public async Task<ResponseMessage<int>> UpdateUnitOfMeasurment(UnitOfMeasurmentDto unitOfMeasurmentDto)
        {
            var response = new ResponseMessage<int>();
            var unitMeasurment = _dBContext.UnitOfMeasurment.Find(unitOfMeasurmentDto.Id);
            if (unitMeasurment == null)
            {
                response.Message = "Unit of Measurment not Found.";
                response.Data = -1;
                response.Success = false;
                response.ErrorCode = HttpStatusCode.NotFound.ToString();
            
                return response;
            }
            unitMeasurment.Name = unitOfMeasurmentDto.Name;
            unitMeasurment.LocalName= unitOfMeasurmentDto.LocalName;
            unitMeasurment.Type = unitOfMeasurmentDto.Type == 0 ? MeasurmentType.percent : MeasurmentType.number;
            unitMeasurment.Remark = unitOfMeasurmentDto.Remark;
            unitMeasurment.RowStatus= unitOfMeasurmentDto.RowStatus== 0?RowStatus.Active:RowStatus.InActive;

            _dBContext.Entry(unitMeasurment).State = EntityState.Modified;
            await _dBContext.SaveChangesAsync();
            response.Message = "Operation Successful.";
            response.Data = -1;
            response.Success = true;
            _logger.LogUpdate("unitOfMeasurmentService",unitOfMeasurmentDto.Id.ToString(), "Unit Of measurement updated Successfully");
            return response;

        }
    }
}
