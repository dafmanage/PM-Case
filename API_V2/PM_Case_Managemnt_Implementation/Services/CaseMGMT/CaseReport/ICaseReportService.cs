using PM_Case_Managemnt_Implementation.DTOS.Case;
using PM_Case_Managemnt_Implementation.Helpers.Response;
using PM_Case_Managemnt_Infrustructure.Models.CaseModel;

namespace PM_Case_Managemnt_Implementation.Services.CaseMGMT
{
    public interface ICaseReportService
    {
        public Task<ResponseMessage<List<CaseReportDto>>> GetCaseReport(Guid subOrgId, string? startAt, string? endAt);
        public Task<ResponseMessage<CaseReportChartDto>> GetCasePieChart(Guid subOrgId, string? startAt, string? endAt);
        public Task<ResponseMessage<CaseReportChartDto>> GetCasePieCharByCaseStatus(Guid subOrgId, string? startAt, string? endAt);
        public Task<ResponseMessage<List<EmployeePerformance>>> GetCaseEmployeePerformace(Guid subOrgId, string key, string OrganizationName);
        public Task<ResponseMessage<List<SMSReportDto>>> GetSMSReport(Guid subOrgId, string? startAt, string? endAt);
        public Task<ResponseMessage<List<CaseDetailReportDto>>> GetCaseDetail(Guid subOrgId, string key);
        public Task<ResponseMessage<CaseProgressReportDto>> GetCaseProgress(Guid CaseNumber);
        public Task<ResponseMessage<List<CaseType>>> GetChildCaseTypes(Guid caseId);
    }
}
