using System.ComponentModel;

namespace PM_Case_Managemnt_Infrustructure.Models.Common
{
    public class CommonModel
    {
        public Guid Id { get; set; }

        public Guid CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }

        [DefaultValue(RowStatus.Active)]
        public RowStatus RowStatus { get; set; }
        public string? Remark { get; set; }
    }

    public enum RowStatus
    {
        Active,
        InActive
    }
}
