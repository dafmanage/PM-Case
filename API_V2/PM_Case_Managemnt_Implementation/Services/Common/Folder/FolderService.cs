using System.Net;
using Microsoft.EntityFrameworkCore;
using PM_Case_Managemnt_Implementation.DTOS.Common.Archive;
using PM_Case_Managemnt_Implementation.Helpers.Logger;
using PM_Case_Managemnt_Implementation.Helpers.Response;
using PM_Case_Managemnt_Infrustructure.Data;
using PM_Case_Managemnt_Infrustructure.Models.Common;

namespace PM_Case_Managemnt_Implementation.Services.Common.FolderService
{
    public class FolderService : IFolderService
    {

        private readonly ApplicationDbContext _dbContext;
        private readonly ILoggerManagerService _logger;

        public FolderService(ApplicationDbContext dbContext, ILoggerManagerService logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<ResponseMessage<int>> Add(FolderPostDto folderPostDto)
        {

            var response = new ResponseMessage<int>();
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
                
                response.Message = "OPeration Successful";
                response.Data = 1;
                response.Success = true;
                _logger.LogCreate("FolderService", folderPostDto.CreatedBy.ToString(), "Folder added Successfully");
                return response;
            }
            catch (Exception ex)
            {
                response.Message = $"{ex.Message}";
                response.Success = false;
                response.Data = -1;
                response.ErrorCode = HttpStatusCode.InternalServerError.ToString();

                return response;
                //throw new Exception(ex.Message);
            }
        }
        public async Task<ResponseMessage<List<FolderGetDto>>> GetAll()
        {
            var response = new ResponseMessage<List<FolderGetDto>>();
            try
            {
                List<FolderGetDto> result = await _dbContext.Folder.Include(x => x.Row.Shelf).Select(x =>
                    new FolderGetDto()
                    {
                        FolderName = x.FolderName,
                        Id = x.Id,
                        Remark = x.Remark,
                        RowId = x.RowId,
                        ShelfId = x.Row.ShelfId,
                        RowNumber = x.Row.RowNumber,
                        ShelfNumber = x.Row.Shelf.ShelfNumber
                    }).ToListAsync();
                response.Message = "Operation Successful.";
                response.Data = result;
                response.Success = true;

                return response;
            } catch (Exception ex)
            {
                response.Message = $"{ex.Message}";
                response.Data = null;
                response.Success = false;
                response.ErrorCode = HttpStatusCode.InternalServerError.ToString();

                return response;
            }
        }

        public async Task<ResponseMessage<List<FolderGetDto>>> GetFilltered(Guid? shelfId = null, Guid? rowId = null)
        {

            var response = new ResponseMessage<List<FolderGetDto>>();
            
            try
            {
                List<FolderGetDto> result = await _dbContext.Folder.Include(x => x.Row.Shelf)
                    .Where(x => x.RowId.Equals(rowId) || !rowId.HasValue)
                    .Where(x => x.Row.ShelfId.Equals(shelfId) || !shelfId.HasValue).Select(x => new FolderGetDto()
                    {
                        FolderName = x.FolderName,
                        Id = x.Id,
                        Remark = x.Remark,
                        RowId = x.RowId,
                        ShelfId = x.Row.ShelfId,
                        RowNumber = x.Row.RowNumber,
                        ShelfNumber = x.Row.Shelf.ShelfNumber
                    }).ToListAsync();

                response.Message = "Operation Successful.";
                response.Success = true;
                response.Data = result;
                
                return response;

            } catch (Exception ex)
            {
                response.Message = $"{ex.Message}";
                response.Data = null;
                response.Success = false;
                response.ErrorCode = HttpStatusCode.InternalServerError.ToString();

                return response;
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
