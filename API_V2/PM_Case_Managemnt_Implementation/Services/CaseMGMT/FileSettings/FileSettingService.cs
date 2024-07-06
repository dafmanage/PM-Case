using Microsoft.EntityFrameworkCore;
using PM_Case_Managemnt_Implementation.DTOS.CaseDto;
using PM_Case_Managemnt_Implementation.Helpers.Response;
using PM_Case_Managemnt_Infrustructure.Data;
using PM_Case_Managemnt_Infrustructure.Models.CaseModel;
using System.Net;

namespace PM_Case_Managemnt_Implementation.Services.CaseService.FileSettings
{
    public class FileSettingService : IFileSettingsService
    {
        private readonly ApplicationDbContext _dbContext;


        public FileSettingService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ResponseMessage<int>> Add(FileSettingPostDto fileSettingPost)
        {
            var response = new ResponseMessage<int>();
            try
            {
                FileSetting fileSetting = new()
                {
                    Id = Guid.NewGuid(),
                    CreatedAt = DateTime.Now,
                    CreatedBy = fileSettingPost.CreatedBy,
                    FileName = fileSettingPost.Name,
                    FileType = Enum.Parse<FileType>(fileSettingPost.FileType),
                    CaseTypeId = fileSettingPost.CaseTypeId,
                };

                await _dbContext.AddAsync(fileSetting);
                await _dbContext.SaveChangesAsync();

                response.Message = "file setting created succesfully.";
                response.Success = true;
                response.Data = 1;

                return response;
            }
            catch (Exception ex)
            {
                response.Message = $"Error creating file setting, {ex.Message}";
                response.Success = false;
                response.Data = 0;
                response.ErrorCode = HttpStatusCode.InternalServerError.ToString();

                return response;
            }
        }

        public async Task<ResponseMessage<int>> UpdateFilesetting(FileSettingPostDto fileSettingPost)
        {
            var response = new ResponseMessage<int>();
            try
            {
                var fileSet = await _dbContext.FileSettings.FindAsync(fileSettingPost.Id);


                fileSet.FileName = fileSettingPost.Name;
                fileSet.FileType = Enum.Parse<FileType>(fileSettingPost.FileType);
                fileSet.CaseTypeId = fileSettingPost.CaseTypeId;

                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return new ResponseMessage<int> { Data = 0, Success = false, Message = ex.ToString() };
            }

            return new ResponseMessage<int> { Data = 1, Success = true, Message = "File Updated Successfully" };
        }


        public async Task<ResponseMessage<int>> DeleteFileSetting(Guid fileId)
        {
            try
            {
                var fileSet = await _dbContext.FileSettings.FindAsync(fileId);

                if (fileSet != null)
                {
                    _dbContext.FileSettings.Remove(fileSet);
                    await _dbContext.SaveChangesAsync();
                }
                else
                {
                    return new ResponseMessage<int> { Data = 0, Success = false, Message = "File Setting Not Found" };
                }


            }
            catch (Exception ex)
            {
                return new ResponseMessage<int> { Data = 0, Success = false, Message = ex.ToString() };
            }

            return new ResponseMessage<int> { Data = 0, Success = true, Message = "File Setting Deleted Successfully" };

        }
        public async Task<ResponseMessage<List<FileSettingGetDto>>> GetAll(Guid subOrgId)
        {
            var response = new ResponseMessage<List<FileSettingGetDto>>();
            try
            {
                List<FileSetting> fileSettings = await _dbContext.FileSettings.Where(x => x.CaseType.SubsidiaryOrganizationId == subOrgId).Include(x => x.CaseType).ToListAsync();
                List<FileSettingGetDto> result = new();

                foreach (FileSetting fileSetting in fileSettings)
                {
                    result.Add(new FileSettingGetDto
                    {
                        Id = fileSetting.Id,
                        CaseTypeTitle = fileSetting.CaseType.CaseTypeTitle,
                        CreatedAt = fileSetting.CreatedAt,
                        CreatedBy = fileSetting.CreatedBy,
                        Name = fileSetting.FileName,
                        FileType = fileSetting.FileType.ToString(),
                        RowStatus = fileSetting.RowStatus.ToString(),
                    });
                }

                response.Message = "retived file setting successfully.";
                response.Success = true;
                response.Data = result;

                return response;

            }
            catch (Exception ex)
            {
                response.Message = $"Error retrieving file settings, {ex.Message}";
                response.Success = false;
                response.Data = null;
                response.ErrorCode = HttpStatusCode.InternalServerError.ToString();

                return response;
            }
        }
    }
}
