using PM_Case_Managemnt_Implementation.DTOS.Common.Archive;

namespace PM_Case_Managemnt_Implementation.Services.Common.FolderService
{
    public interface IFolderService
    {
        public Task Add(FolderPostDto folderPostDto);
        public Task<List<FolderGetDto>> GetAll();
        public Task<List<FolderGetDto>> GetFilltered(Guid? shelfId = null, Guid? rowId = null);

    }
}
