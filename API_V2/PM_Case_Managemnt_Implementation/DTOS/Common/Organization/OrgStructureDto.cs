
namespace PM_Case_Managemnt_Implementation.DTOS.Common
{
    public class OrgStructureDto
    {
        public Guid? Id { get; set; }
        public Guid OrganizationBranchId { get; set; }
        public string? BranchName { get; set; }
        public bool IsBranch { get; set; } = false;
        public Guid? ParentStructureId { get; set; }
        public string? ParentStructureName { get; set; }
        public string StructureName { get; set; }
        public float? ParentWeight { get; set; }
        public int Order { get; set; }
        public float Weight { get; set; }
        public string Remark { get; set; }
        public string? OfficeNumber { get; set; }
        public int RowStatus { get; set; }
        public Guid SubsidiaryOrganizationId { get; set; }
    }



    public class DiagramDto
    {
        public string? Label { get; set; }
        public dynamic Data { get; set; }
        public List<DiagramDto> Children { get; set; }
        public bool Expanded { get; set; }
        public string? Type { get; set; }
        public string? StyleClass { get; set; }
        public Guid? Id { get; set; }
        public Guid? ParentId { get; set; }
        public int Order { get; set; }
    }
}
