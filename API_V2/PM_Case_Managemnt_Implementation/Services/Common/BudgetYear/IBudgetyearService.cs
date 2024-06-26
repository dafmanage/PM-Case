using PM_Case_Managemnt_Implementation.DTOS.Common;
using PM_Case_Managemnt_Implementation.Helpers;
using PM_Case_Managemnt_Infrustructure.Models.Common;

namespace PM_Case_Managemnt_Implementation.Services.Common
{
    public interface IBudgetyearService
    {
        //Program Budget Year
        Task<ResponseMessage> CreateProgramBudgetYear(ProgramBudgetYearDto programBudgetYear);

        Task<ResponseMessage> EditProgramBudgetYear(ProgramBudgetYearDto programBudgetYear);

        Task<ResponseMessage> DeleteProgramBudgetYear(Guid programBudgetYeatId);
        Task<List<ProgramBudgetYear>> GetProgramBudgetYears(Guid subOrgId);
        Task<List<SelectListDto>> getProgramBudgetSelectList(Guid subOrgId);


        // Budget Year
        Task<ResponseMessage> CreateBudgetYear(BudgetYearDto BudgetYear);
        Task<ResponseMessage> EditBudgetYear(BudgetYearDto BudgetYear);
        Task<ResponseMessage> DeleteBudgetYear(Guid budgetYearId);
        Task<List<BudgetYearDto>> GetBudgetYears(Guid programBudgetYearId);
        Task<List<SelectListDto>> GetBudgetYearsFromProgramId(Guid ProgramId);
    }
}
