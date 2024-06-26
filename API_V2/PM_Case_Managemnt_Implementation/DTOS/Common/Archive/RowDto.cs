namespace PM_Case_Managemnt_Implementation.DTOS.Common.Archive
{
    public class RowPostDto
    {
        public string RowNumber { get; set; }
        public Guid ShelfId { get; set; }
        public string Remark { get; set; }
        public Guid CreatedBy { get; set; }
    }

    public class RowGetDto
    {
        public Guid Id { get; set; }
        public Guid ShelfId { get; set; }
        public string RowNumber { get; set; }
        public string ShelfNumber { get; set; }
        public string Remark { get; set; }
    }
}
