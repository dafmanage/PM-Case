using PM_Case_Managemnt_API.Models.KPI;
using PM_Case_Managemnt_Infrustructure.Models.Common;

namespace PM_Case_Managemnt_Infrustructure.Models.KPI
{
    public class KPIData : CommonModel
    {
        public Guid KPIDetailId { get; set; }
        public KPIDetails KPIDetail { get; set; }
        public int Year { get; set; }
        public string Data { get; set; }
    }
}
