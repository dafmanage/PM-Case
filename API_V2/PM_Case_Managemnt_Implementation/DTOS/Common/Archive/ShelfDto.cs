namespace PM_Case_Managemnt_Implementation.DTOS.Common.Archive
{

    public class ShelfPostDto
    {
        public string ShelfNumber { get; set; }
        public string Remark { get; set; }
        public Guid CreatedBy { get; set; }
        public Guid SubsidiaryOrganizationId { get; set; }
    }

    public class ShelfGetDto
    {
        public Guid Id { get; set; }
        public string ShelfNumber { get; set; }
        public string Remark { get; set; }
    }
}
