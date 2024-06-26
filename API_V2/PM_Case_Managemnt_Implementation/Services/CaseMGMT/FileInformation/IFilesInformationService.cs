using PM_Case_Managemnt_Infrustructure.Models.CaseModel;

namespace PM_Case_Managemnt_Implementation.Services.CaseMGMT.FileInformationService
{
    public interface IFilesInformationService
    {
        public Task AddMany(List<FilesInformation> fileInformations);
    }
}
