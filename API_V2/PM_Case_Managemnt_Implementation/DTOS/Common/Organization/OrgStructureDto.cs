
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

        public string? label { get; set; }
        public dynamic data { get; set; }

        public List<DiagramDto> children { get; set; }

        public bool expanded { get; set; }

        public string? type { get; set; }

        public string? styleClass { get; set; }

        public Guid? id { get; set; }

        public Guid? parentId { get; set; }

        public int order { get; set; }



    }
}
