using Microsoft.EntityFrameworkCore;
using PM_Case_Managemnt_Implementation.DTOS.Common.Archive;
using PM_Case_Managemnt_Infrustructure.Data;
using PM_Case_Managemnt_Infrustructure.Models.Common;

namespace PM_Case_Managemnt_Implementation.Services.Common.RowService
{
    public class RowService : IRowService
    {
        private readonly DBContext _dbContext;

        public RowService(DBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Add(RowPostDto rowPost)
        {
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
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<RowGetDto>> GetAll(Guid shelfId)
        {
            try
            {
                return (await _dbContext.Rows.Where(x => x.ShelfId == shelfId).Include(x => x.Shelf).Select(x => new RowGetDto()
                {
                    Id = x.Id,
                    Remark = x.Remark,
                    RowNumber = x.RowNumber,
                    ShelfId = x.ShelfId,
                    ShelfNumber = x.Shelf.ShelfNumber
                }).ToListAsync());
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
