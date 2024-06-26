

using PM_Case_Managemnt_Infrustructure.Models.Common.Organization;

namespace PM_Case_Managemnt_Infrustructure.Models.Common
{
    public class UnitOfMeasurment : CommonModel
    {
        public string Name { get; set; } = null!;

        public string LocalName { get; set; } = null!;
        public MeasurmentType Type { get; set; }
        public Guid SubsidiaryOrganizationId { get; set; }
        public virtual SubsidiaryOrganization? SubsidiaryOrganization { get; set; }

    }

    public enum MeasurmentType
    {
        percent,
        number
    }
}
