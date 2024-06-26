using PM_Case_Managemnt_Implementation.DTOS.Common.Archive;

namespace PM_Case_Managemnt_Implementation.Services.Common.RowService
{
    public interface IRowService
    {
        public Task Add(RowPostDto rowPostDto);
        public Task<List<RowGetDto>> GetAll(Guid shelfId);
    }
}
