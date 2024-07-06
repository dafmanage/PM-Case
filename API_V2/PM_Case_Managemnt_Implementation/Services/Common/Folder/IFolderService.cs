using PM_Case_Managemnt_Implementation.DTOS.Common.Archive;
using PM_Case_Managemnt_Implementation.Helpers.Response;

namespace PM_Case_Managemnt_Implementation.Services.Common.FolderService
{
    public interface IFolderService
    {
        public Task<ResponseMessage<int>> Add(FolderPostDto folderPostDto);
        public Task<ResponseMessage<List<FolderGetDto>>> GetAll();
        public Task<ResponseMessage<List<FolderGetDto>>> GetFilltered(Guid? shelfId = null, Guid? rowId = null);

    }
}
