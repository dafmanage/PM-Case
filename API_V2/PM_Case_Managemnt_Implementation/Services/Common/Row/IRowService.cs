using PM_Case_Managemnt_Implementation.DTOS.Common.Archive;
using PM_Case_Managemnt_Implementation.Helpers.Response;

namespace PM_Case_Managemnt_Implementation.Services.Common.RowService
{
    public interface IRowService
    {
        public Task<ResponseMessage<int>> Add(RowPostDto rowPostDto);
        public Task<ResponseMessage<List<RowGetDto>>> GetAll(Guid shelfId);
    }
}
