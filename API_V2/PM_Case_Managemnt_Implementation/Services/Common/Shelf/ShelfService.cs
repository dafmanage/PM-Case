using Microsoft.EntityFrameworkCore;
using PM_Case_Managemnt_Implementation.DTOS.Common.Archive;
using PM_Case_Managemnt_Infrustructure.Data;
using PM_Case_Managemnt_Infrustructure.Models.Common;

namespace PM_Case_Managemnt_Implementation.Services.Common.ShelfService
{
    public class ShelfService : IShelfService
    {
        private readonly ApplicationDbContext _dbContext;

        public ShelfService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Add(ShelfPostDto shelfPostDto)
        {
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
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }


        public async Task<List<ShelfGetDto>> GetAll(Guid subOrgId)
        {
            try
            {
                return (await _dbContext.Shelf.Where(x => x.SubsidiaryOrganizationId == subOrgId).Select(x => new ShelfGetDto()
                {
                    Id = x.Id,
                    Remark = x.Remark,
                    ShelfNumber = x.ShelfNumber
                }).ToListAsync());
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
