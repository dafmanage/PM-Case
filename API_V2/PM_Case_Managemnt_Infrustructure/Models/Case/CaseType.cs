
using PM_Case_Managemnt_Infrustructure.Models.Common;
using PM_Case_Managemnt_Infrustructure.Models.Common.Organization;

namespace PM_Case_Managemnt_Infrustructure.Models.CaseModel
{
    public class CaseType : CommonModel
    {

        public CaseType()
        {
            Childrens = new HashSet<CaseType>();
        }

        public string CaseTypeTitle { get; set; } = null!;


        public string? Code { get; set; }


        public float TotlaPayment { get; set; }

        public float Counter { get; set; }
        public Guid? ParentCaseTypeId { get; set; }
        public virtual CaseType ParentCaseType { get; set; } = null!;
        public int? OrderNumber { get; set; }
        public TimeMeasurement MeasurementUnit { get; set; }
        public CaseForm? CaseForm { get; set; }
        public virtual SubsidiaryOrganization? SubsidiaryOrganization { get; set; }
        public Guid SubsidiaryOrganizationId { get; set; }

        public virtual ICollection<CaseType> Childrens { get; set; }

    }





    public enum TimeMeasurement
    {
        Minutes,
        Hour,
        Day
    }

    public enum CaseForm
    {
        Outside,
        Inside
    }
}
