namespace PM_Case_Managemnt_Implementation.DTOS.Common
{
    public class OrgBranchDto
    {
        public Guid? Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }

        public int RowStatus { get; set; }
        public string Remark { get; set; }

    }
}
