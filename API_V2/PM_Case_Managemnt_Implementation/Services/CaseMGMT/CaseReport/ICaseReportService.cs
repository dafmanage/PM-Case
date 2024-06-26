using PM_Case_Managemnt_Implementation.DTOS.Case;
using PM_Case_Managemnt_Infrustructure.Models.CaseModel;

namespace PM_Case_Managemnt_Implementation.Services.CaseMGMT
{
    public interface ICaseReportService
    {

        public Task<List<CaseReportDto>> GetCaseReport(Guid subOrgId, string? startAt, string? endAt);
        public Task<CaseReportChartDto> GetCasePieChart(Guid subOrgId, string? startAt, string? endAt);
        public Task<CaseReportChartDto> GetCasePieCharByCaseStatus(Guid subOrgId, string? startAt, string? endAt);
        public Task<List<EmployeePerformance>> GetCaseEmployeePerformace(Guid subOrgId, string key, string OrganizationName);
        public Task<List<SMSReportDto>> GetSMSReport(Guid subOrgId, string? startAt, string? endAt);
        public Task<List<CaseDetailReportDto>> GetCaseDetail(Guid subOrgId, string key);
        public Task<CaseProgressReportDto> GetCaseProgress(Guid CaseNumber);
        public Task<List<CaseType>> GetChildCaseTypes(Guid caseId);

    }
}
