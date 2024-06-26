using Microsoft.EntityFrameworkCore;

namespace PM_Case_Managemnt_Infrustructure.Models.Common
{
    [Index(nameof(RowNumber), nameof(ShelfId), IsUnique = true)]
    public class Row : CommonModel
    {
        public string RowNumber { get; set; } = null!;
        public Guid ShelfId { get; set; }
        public virtual Shelf Shelf { get; set; }
    }
}
