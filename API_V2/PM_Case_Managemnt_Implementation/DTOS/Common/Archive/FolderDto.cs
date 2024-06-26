namespace PM_Case_Managemnt_Implementation.DTOS.Common.Archive
{
    public class FolderPostDto
    {
        public string FolderName { get; set; }
        public Guid RowId { get; set; }
        public string Remark { get; set; }
        public Guid CreatedBy { get; set; }
    }

    public class FolderGetDto
    {
        public Guid Id { get; set; }
        public Guid ShelfId { get; set; }
        public Guid RowId { get; set; }
        public string FolderName { get; set; }
        public string RowNumber { get; set; }
        public string ShelfNumber { get; set; }
        public string Remark { get; set; }
    }
}
