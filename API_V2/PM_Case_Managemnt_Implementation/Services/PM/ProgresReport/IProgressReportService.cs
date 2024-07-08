using PM_Case_Managemnt_Implementation.DTOS.Common;
using PM_Case_Managemnt_Implementation.DTOS.PM;

namespace PM_Case_Managemnt_Implementation.Services.PM.ProgresReport
{
    public interface IProgressReportService
    {
        public Task<List<DiagramDto>> GetDirectorLevelPerformance(Guid subOrgId, Guid? BranchId);    
        public Task<PlanReportByProgramDto> PlanReportByProgram(Guid subOrgID, string BudgetYear, string ReportBy);
        public Task<PlanReportDetailDto> StructureReportByProgram(string BudgetYear, string ProgramId, string ReportBy);
        public Task<PlannedReport> PlanReports(string budgetYear, Guid selectStructureId, string ReportBy);
        public ProgressReport GenerateProgressReport(FilterationCriteria filterationCriteria);
        public ProgresseReportByStructure GetProgressByStructure(int BudgetYear, Guid selectStructureId, string ReportBy);
        public Task<PerformanceReport> PerformanceReports(FilterationCriteria filterationCriteria);
        public Task<List<ActivityProgressViewModel>> GetActivityProgress(Guid? activityId);
        public Task<List<EstimatedCostDto>> GetEstimatedCost(Guid structureId, int budegtYear);
    }
}
