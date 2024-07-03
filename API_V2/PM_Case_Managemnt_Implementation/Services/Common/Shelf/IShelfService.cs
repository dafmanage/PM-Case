using PM_Case_Managemnt_Implementation.DTOS.Common.Archive;
using PM_Case_Managemnt_Implementation.Helpers.Response;

namespace PM_Case_Managemnt_Implementation.Services.Common.ShelfService
{
    public interface IShelfService
    {
        public Task<ResponseMessage<int>> Add(ShelfPostDto shelfPostDto);
        public Task<ResponseMessage<List<ShelfGetDto>>> GetAll(Guid subOrgId);
    }
}
