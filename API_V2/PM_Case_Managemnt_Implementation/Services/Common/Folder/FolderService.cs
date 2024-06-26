using Microsoft.EntityFrameworkCore;
using PM_Case_Managemnt_Implementation.DTOS.Common.Archive;
using PM_Case_Managemnt_Infrustructure.Data;
using PM_Case_Managemnt_Infrustructure.Models.Common;

namespace PM_Case_Managemnt_Implementation.Services.Common.FolderService
{
    public class FolderService : IFolderService
    {

        private readonly DBContext _dbContext;

        public FolderService(DBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Add(FolderPostDto folderPostDto)
        {
            try
            {
                Folder newFolder = new()
                {
                    Id = Guid.NewGuid(),
                    CreatedAt = DateTime.Now,
                    RowStatus = RowStatus.Active,
                    CreatedBy = folderPostDto.CreatedBy,
                    FolderName = folderPostDto.FolderName,
                    Remark = folderPostDto.Remark,
                    RowId = folderPostDto.RowId,
                };

                await _dbContext.Folder.AddAsync(newFolder);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task<List<FolderGetDto>> GetAll()
        {
            try
            {
                return (await _dbContext.Folder.Include(x => x.Row.Shelf).Select(x => new FolderGetDto()
                {
                    FolderName = x.FolderName,
                    Id = x.Id,
                    Remark = x.Remark,
                    RowId = x.RowId,
                    ShelfId = x.Row.ShelfId,
                    RowNumber = x.Row.RowNumber,
                    ShelfNumber = x.Row.Shelf.ShelfNumber
                }).ToListAsync());
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<FolderGetDto>> GetFilltered(Guid? shelfId = null, Guid? rowId = null)
        {
            try
            {
                return (await _dbContext.Folder.Include(x => x.Row.Shelf).Where(x => x.RowId.Equals(rowId) || !rowId.HasValue).Where(x => x.Row.ShelfId.Equals(shelfId) || !shelfId.HasValue).Select(x => new FolderGetDto()
                {
                    FolderName = x.FolderName,
                    Id = x.Id,
                    Remark = x.Remark,
                    RowId = x.RowId,
                    ShelfId = x.Row.ShelfId,
                    RowNumber = x.Row.RowNumber,
                    ShelfNumber = x.Row.Shelf.ShelfNumber
                }).ToListAsync());

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        //public async Task<List<FolderGetDto>> GetByRowId(Guid rowId)
        //{
        //    try
        //    {

        //    } catch (Exception ex)
        //    {
        //        throw new Exception(ex.Message);
        //    }
        //}
    }
}
