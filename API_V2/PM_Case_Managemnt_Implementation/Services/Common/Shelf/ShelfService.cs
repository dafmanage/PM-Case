using System.Net;
using Microsoft.EntityFrameworkCore;
using PM_Case_Managemnt_Implementation.DTOS.Common.Archive;
using PM_Case_Managemnt_Implementation.Helpers.Logger;
using PM_Case_Managemnt_Implementation.Helpers.Response;
using PM_Case_Managemnt_Infrustructure.Data;
using PM_Case_Managemnt_Infrustructure.Models.Common;

namespace PM_Case_Managemnt_Implementation.Services.Common.ShelfService
{
    public class ShelfService : IShelfService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILoggerManagerService _logger;
        public ShelfService(ApplicationDbContext dbContext, ILoggerManagerService logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<ResponseMessage<int>> Add(ShelfPostDto shelfPostDto)
        {
            var response = new ResponseMessage<int>();
            
            try
            {
                Shelf newShelf = new()
                {
                    Id = Guid.NewGuid(),
                    CreatedAt = DateTime.Now,
                    CreatedBy = shelfPostDto.CreatedBy,
                    Remark = shelfPostDto.Remark,
                    RowStatus = RowStatus.Active,
                    ShelfNumber = shelfPostDto.ShelfNumber,
                    SubsidiaryOrganizationId = shelfPostDto.SubsidiaryOrganizationId
                };

                await _dbContext.Shelf.AddAsync(newShelf);
                await _dbContext.SaveChangesAsync();

                response.Message = "operation Successful.";
                response.Success = true;
                response.Data = 1;
                _logger.LogCreate("ShelfService", shelfPostDto.CreatedBy.ToString(), "Shelf added Successfully");
                return response;
                
            } catch (Exception ex)
            {
               
                response.Message = $"{ex.Message}";
                response.Data = -1;
                response.Success = false;
                response.ErrorCode = HttpStatusCode.InternalServerError.ToString();

                return response;
            }

        }


        public async Task<ResponseMessage<List<ShelfGetDto>>> GetAll(Guid subOrgId)
        {
            var response = new ResponseMessage<List<ShelfGetDto>>();
            try
            {
                List<ShelfGetDto> result = await _dbContext.Shelf.Where(x => x.SubsidiaryOrganizationId == subOrgId).Select(x => new ShelfGetDto()
                {
                    Id = x.Id, 
                    Remark = x.Remark, 
                    ShelfNumber = x.ShelfNumber
                }).ToListAsync();

                response.Message = "Operation Successful.";
                response.Data = result;
                response.Success = true;

                return response;
            }
            catch (Exception ex)
            {
                response.Message = $"{ex.Message}";
                response.Data = null;
                response.Success = false;
                response.ErrorCode = HttpStatusCode.InternalServerError.ToString();

                return response;
            }
        }
    }
}
