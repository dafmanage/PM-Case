using Microsoft.EntityFrameworkCore;

namespace PM_Case_Managemnt_Infrustructure.Models.Common
{
    [Index(nameof(FolderName), nameof(RowId), IsUnique = true)]
    public class Folder : CommonModel
    {
        public string FolderName { get; set; } = null!;

        //public Guid ShelfId { get; set; }

        //public virtual Shelf? Shelf { get; set; }

        public Guid RowId { get; set; }
        public virtual Row? Row { get; set; }
    }

}
