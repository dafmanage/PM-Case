
namespace PM_Case_Managemnt_Infrustructure.Models.Common
{
    public class BudgetYear : CommonModel
    {
        public int Year { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public Guid ProgramBudgetYearId { get; set; }
    }
}
