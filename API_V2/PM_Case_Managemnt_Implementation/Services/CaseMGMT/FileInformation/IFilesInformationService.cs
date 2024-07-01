using PM_Case_Managemnt_Implementation.Helpers.Response;
using PM_Case_Managemnt_Infrustructure.Models.CaseModel;

namespace PM_Case_Managemnt_Implementation.Services.CaseMGMT.FileInformationService
{
    public interface IFilesInformationService
    {
        public Task<ResponseMessage<int>> AddMany(List<FilesInformation> fileInformations);
    }
}
