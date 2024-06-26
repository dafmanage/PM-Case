namespace PM_Case_Managemnt_Implementation.DTOS.Common
{
    public class UnitOfMeasurmentDto
    {

        public Guid? Id { get; set; }
        public string Name { get; set; }
        public string LocalName { get; set; }
        public int Type { get; set; }
        public string? Remark { get; set; }
        public int RowStatus { get; set; }
        public Guid SubsidiaryOrganizationId { get; set; }
    }
}
