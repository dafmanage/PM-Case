using PM_Case_Managemnt_Implementation.DTOS.Common;
using static PM_Case_Managemnt_Implementation.Services.PM.ProgressReport.ProgressReportService;

namespace PM_Case_Managemnt_Implementation.Services.PM.ProgressReport
{
    public interface IProgressReportService
    {

        public Task<List<DiagramDto>> GetDirectorLevelPerformance(Guid subOrgId, Guid? BranchId);
        public Task<PlanReportByProgramDto> PlanReportByProgram(Guid subOrgID, string BudgetYear, string ReportBy);

        public Task<PlanReportDetailDto> StructureReportByProgram(string BudgetYear, string ProgramId, string ReportBy);

        public Task<PlannedReport> PlanReports(string BudgetYea, Guid selectStructureId, string ReportBy);
        public Task<ProgresseReport> ProgresssReport(FilterationCriteria filterationCriteria);

        public Task<ProgresseReportByStructure> GetProgressByStructure(int BudgetYea, Guid selectStructureId, string ReportBy);

        public Task<PerfomanceReport> PerformanceReports(FilterationCriteria filterationCriteria);

        public Task<List<ActivityProgressViewModel>> GetActivityProgress(Guid? activityId);

        public Task<List<EstimatedCostDto>> GetEstimatedCost(Guid structureId, int budegtYear);


    }
}
