using Microsoft.AspNetCore.Mvc;
using PM_Case_Managemnt_Implementation.Services.CaseMGMT;
using PM_Case_Managemnt_Infrustructure.Data;

namespace PM_Case_Managemnt_API.Controllers.Case
{
    [Route("api/case/[controller]")]
    [ApiController]
    public class CaseREportController : ControllerBase
    {
        private readonly ICaseReportService _caserReportService;
        private readonly DBContext _dbContext;
        public CaseREportController(ICaseReportService caseReportService, DBContext dBContext)
        {
            _caserReportService = caseReportService;
            _dbContext = dBContext;
        }


        [HttpGet("GetCaseReport")]

        public async Task<IActionResult> GetCaseReport(Guid subOrgId, string? startAt, string? endAt)
        {

            try
            {
                return Ok(await _caserReportService.GetCaseReport(subOrgId, startAt, endAt));
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error");
            }

        }

        [HttpGet("GetCasePieChart")]

        public async Task<IActionResult> GetCasePieChart(Guid subOrgId, string? startAt, string? endAt)
        {

            try
            {
                return Ok(await _caserReportService.GetCasePieChart(subOrgId, startAt, endAt));
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error");
            }




        }

        [HttpGet("GetCasePieChartByStatus")]

        public async Task<IActionResult> GetCasePieCharByCaseStatus(Guid subOrgId, string? startAt, string? endAt)
        {
            try
            {
                return Ok(await _caserReportService.GetCasePieCharByCaseStatus(subOrgId, startAt, endAt));
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpGet("GetCaseEmployeePerformace")]

        public async Task<IActionResult> GetCaseEmployeePerformace(Guid subOrgId, string? key, string? OrganizationName)
        {
            try
            {
                return Ok(await _caserReportService.GetCaseEmployeePerformace(subOrgId, key, OrganizationName));
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpGet("GetSMSReport")]

        public async Task<IActionResult> GetSMSReport(Guid subOrgId, string? startAt, string? endAt)
        {

            try
            {

                return Ok(await _caserReportService.GetSMSReport(subOrgId, startAt, endAt));
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpGet("GetCaseDetail")]
        public async Task<IActionResult> GetCaseDetail(Guid subOrgId, string? key)
        {

            try
            {
                return Ok(await _caserReportService.GetCaseDetail(subOrgId, key));
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error");
            }


        }

        [HttpGet("GetCaseDetailProgress")]
        public async Task<IActionResult> GetCaseDetailProgress(Guid caseId)
        {

            try
            {
                return Ok(await _caserReportService.GetCaseProgress(caseId));
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error");
            }


        }

        [HttpGet("GetCaseTypes")]

        public async Task<IActionResult> GetChildCaseTypes(Guid caseId)
        {
            try
            {
                return Ok(await _caserReportService.GetChildCaseTypes(caseId));
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error");
            }


        }


    }
}
