using PM_Case_Managemnt_Implementation.DTOS.Common;
using PM_Case_Managemnt_Implementation.Helpers.Response;
using static PM_Case_Managemnt_Implementation.Services.PM.ProgressReport.ProgressReportService;

namespace PM_Case_Managemnt_Implementation.Services.PM.ProgressReport
{
    public interface IProgressReportService
    {

        public Task<ResponseMessage<List<DiagramDto>>> GetDirectorLevelPerformance(Guid subOrgId, Guid? BranchId);
        public Task<ResponseMessage<PlanReportByProgramDto>> PlanReportByProgram(Guid subOrgID, string BudgetYear, string ReportBy);

        public Task<ResponseMessage<PlanReportDetailDto>> StructureReportByProgram(string BudgetYear, string ProgramId, string ReportBy);

        public Task<ResponseMessage<PlannedReport>> PlanReports(string BudgetYea, Guid selectStructureId, string ReportBy);
        public Task<ResponseMessage<ProgresseReport>> ProgresssReport(FilterationCriteria filterationCriteria);

        public Task<ResponseMessage<ProgresseReportByStructure>> GetProgressByStructure(int BudgetYea, Guid selectStructureId, string ReportBy);

        public Task<ResponseMessage<PerfomanceReport>> PerformanceReports(FilterationCriteria filterationCriteria);

        public Task<ResponseMessage<List<ActivityProgressViewModel>>> GetActivityProgress(Guid? activityId);

        public Task<ResponseMessage<List<EstimatedCostDto>>> GetEstimatedCost(Guid structureId, int budegtYear);


    }
}
