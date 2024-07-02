using System.Net;
using Microsoft.EntityFrameworkCore;
using PM_Case_Managemnt_Implementation.DTOS.Common.Archive;
using PM_Case_Managemnt_Implementation.Helpers.Response;
using PM_Case_Managemnt_Infrustructure.Data;
using PM_Case_Managemnt_Infrustructure.Models.Common;

namespace PM_Case_Managemnt_Implementation.Services.Common.RowService
{
    public class RowService : IRowService
    {
        private readonly ApplicationDbContext _dbContext;

        public RowService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ResponseMessage<int>> Add(RowPostDto rowPost)
        {
            var response = new ResponseMessage<int>();
            
            try
            {
                Row currRow = new()
                {
                    Id = Guid.NewGuid(),
                    CreatedAt = DateTime.Now,
                    RowStatus = RowStatus.Active,
                    CreatedBy = rowPost.CreatedBy,
                    Remark = rowPost.Remark,
                    RowNumber = rowPost.RowNumber,
                    ShelfId = rowPost.ShelfId
                };


                await _dbContext.Rows.AddAsync(currRow);
                await _dbContext.SaveChangesAsync();

                response.Message = "Operation Successful.";
                response.Data = 1;
                response.Success = true;

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

        public async Task<ResponseMessage<List<RowGetDto>>> GetAll( Guid shelfId)
        {
            var response = new ResponseMessage<List<RowGetDto>>();
            try
            {
                List<RowGetDto> result =  await _dbContext.Rows.Where(x=>x.ShelfId == shelfId).Include(x => x.Shelf).Select(x => new RowGetDto()
                {
                    Id = x.Id,
                    Remark = x.Remark,
                    RowNumber = x.RowNumber,
                    ShelfId= x.ShelfId,
                    ShelfNumber = x.Shelf.ShelfNumber
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
