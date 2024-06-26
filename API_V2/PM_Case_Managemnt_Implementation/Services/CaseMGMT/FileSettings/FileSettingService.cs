using Microsoft.EntityFrameworkCore;
using PM_Case_Managemnt_Implementation.DTOS.CaseDto;
using PM_Case_Managemnt_Implementation.Helpers;
using PM_Case_Managemnt_Infrustructure.Data;
using PM_Case_Managemnt_Infrustructure.Models.CaseModel;

namespace PM_Case_Managemnt_Implementation.Services.CaseService.FileSettings
{
    public class FileSettingService : IFileSettingsService
    {
        private readonly DBContext _dbContext;


        public FileSettingService(DBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Add(FileSettingPostDto fileSettingPost)
        {
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
            }
            catch (Exception ex)
            {
                throw new Exception("Error creating file setting");
            }
        }

        public async Task<ResponseMessage> UpdateFilesetting(FileSettingPostDto fileSettingPost)
        {
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
                return new ResponseMessage { Success = false, Message = ex.ToString() };
            }

            return new ResponseMessage { Success = true, Message = "File Updated Successfully" };
        }


        public async Task<ResponseMessage> DeleteFileSetting(Guid fileId)
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
                    return new ResponseMessage { Success = false, Message = "File Setting Not Found" };
                }


            }
            catch (Exception ex)
            {
                return new ResponseMessage { Success = false, Message = ex.ToString() };
            }

            return new ResponseMessage { Success = true, Message = "File Setting Deleted Successfully" };

        }
        public async Task<List<FileSettingGetDto>> GetAll(Guid subOrgId)
        {
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

                return result;

            }
            catch (Exception ex)
            {
                throw new Exception("Error Retrieving File settings");
            }
        }
    }
}
