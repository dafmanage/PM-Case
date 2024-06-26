using PM_Case_Managemnt_Implementation.DTOS.Common.Archive;

namespace PM_Case_Managemnt_Implementation.Services.Common.ShelfService
{
    public interface IShelfService
    {
        public Task Add(ShelfPostDto shelfPostDto);
        public Task<List<ShelfGetDto>> GetAll(Guid subOrgId);
    }
}
