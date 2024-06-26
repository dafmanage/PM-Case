using PM_Case_Managemnt_Implementation.DTOS.CaseDto;
using PM_Case_Managemnt_Implementation.Helpers;

namespace PM_Case_Managemnt_Implementation.Services.CaseService.FileSettings
{
    public interface IFileSettingsService
    {
        public Task Add(FileSettingPostDto fileSettingPost);
        public Task<ResponseMessage> UpdateFilesetting(FileSettingPostDto fileSettingPost);
        public Task<ResponseMessage> DeleteFileSetting(Guid fileId);
        public Task<List<FileSettingGetDto>> GetAll(Guid subOrgId);
    }
}
