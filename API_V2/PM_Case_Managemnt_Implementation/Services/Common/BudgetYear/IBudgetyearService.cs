using PM_Case_Managemnt_Implementation.DTOS.Common;
using PM_Case_Managemnt_Implementation.Helpers.Response;
using PM_Case_Managemnt_Infrustructure.Models.Common;

namespace PM_Case_Managemnt_Implementation.Services.Common
{
    public interface IBudgetyearService
    {
        //Program Budget Year
        Task<ResponseMessage<int>> CreateProgramBudgetYear(ProgramBudgetYearDto programBudgetYear);

        Task<ResponseMessage<int>> EditProgramBudgetYear(ProgramBudgetYearDto programBudgetYear);

        Task<ResponseMessage<int>> DeleteProgramBudgetYear(Guid programBudgetYeatId);
        Task<List<ProgramBudgetYear>> GetProgramBudgetYears(Guid subOrgId);
        Task<List<SelectListDto>> getProgramBudgetSelectList(Guid subOrgId);


        // Budget Year
        Task<ResponseMessage<int>> CreateBudgetYear(BudgetYearDto BudgetYear);
        Task<ResponseMessage<int>> EditBudgetYear(BudgetYearDto BudgetYear);
        Task<ResponseMessage<int>> DeleteBudgetYear(Guid budgetYearId);
        Task<List<BudgetYearDto>> GetBudgetYears(Guid programBudgetYearId);
        Task<List<SelectListDto>> GetBudgetYearsFromProgramId(Guid ProgramId);
    }
}
