using PM_Case_Managemnt_Implementation.DTOS.CaseDto;
using PM_Case_Managemnt_Implementation.Helpers.Response;

namespace PM_Case_Managemnt_Implementation.Services.CaseService.FileSettings
{
    public interface IFileSettingsService
    {
        public Task<ResponseMessage<int>> Add(FileSettingPostDto fileSettingPost);
        public Task<ResponseMessage<int>> UpdateFilesetting(FileSettingPostDto fileSettingPost);
        public Task<ResponseMessage<int>> DeleteFileSetting(Guid fileId);
        public Task<ResponseMessage<List<FileSettingGetDto>>> GetAll(Guid subOrgId);
    }
}
