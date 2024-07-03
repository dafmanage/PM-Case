using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PM_Case_Managemnt_Implementation.DTOS.CaseDto;
using PM_Case_Managemnt_Implementation.Services.CaseMGMT;
using PM_Case_Managemnt_Implementation.Services.CaseMGMT.Applicants;
using PM_Case_Managemnt_Infrustructure.Data;
using PM_Case_Managemnt_Infrustructure.Models.CaseModel;
using PM_Case_Managemnt_Infrustructure.Models.Common;
using System.Net.Http.Headers;
using PM_Case_Managemnt_API.Services.CaseMGMT.Applicants;

namespace PM_Case_Managemnt_API.Controllers.Case
{
    [Route("api/case")]
    [ApiController]
    public class CaseProccessingController : ControllerBase
    {
        private readonly ICaseProccessingService _caseProcessingService;
        private readonly ApplicationDbContext _dbContext;
        private readonly IApplicantServices _applicantService;

        public CaseProccessingController(ICaseProccessingService caseProccessingService, ApplicationDbContext dBContext, IApplicantServices applicantService)
        {
            _caseProcessingService = caseProccessingService;
            _dbContext = dBContext;
            _applicantService = applicantService;
        }


        [HttpPut("confirm")]

        public async Task<IActionResult> ConfirmCase(ConfirmTranscationDto confirmTranscationDto)
        {

            try
            {
                return Ok(await _caseProcessingService.ConfirmTranasaction(confirmTranscationDto));
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error");
            }

        }

        [HttpGet("getCaseDetail")]

        public async Task<IActionResult> GetCaseDetail(Guid EmployeeId, Guid CaseHistoryId)
        {
            try
            {
                return Ok(await _caseProcessingService.GetCaseDetial(EmployeeId, CaseHistoryId));
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error");
            }

        }
        [HttpPost("assign")]
        public async Task<IActionResult> AssignCase(CaseAssignDto caseAssignDto)
        {
            try
            {
                await _caseProcessingService.AssignTask(caseAssignDto);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpPost("complete")]
        public async Task<IActionResult> CompleteCase()
        {
            try
            {

                CaseCompleteDto caseCompeleteDto = new()
                {
                    CaseHistoryId = Guid.Parse(Request.Form["CaseHistoryId"]),
                    EmployeeId = Guid.Parse(Request.Form["EmployeeId"]),
                    Remark = Request.Form["Remark"]
                };
                var history = await _dbContext.CaseHistories.Where(x => x.Id == caseCompeleteDto.CaseHistoryId).Include(x => x.Case).FirstOrDefaultAsync();

                caseCompeleteDto.CaseAttachments = new List<CaseAttachment>();

                foreach (var file in Request.Form.Files)
                {

                    if (file.Name.ToLower() == "attachments")
                    {
                        string folderName = Path.Combine("Assets", "CaseAttachments");
                        string applicantName;

                        if (history.Case.ApplicantId != null)
                        {
                            var applicant =await _applicantService.GetApplicantById(history.Case.ApplicantId);
                            applicantName = applicant.Data.ApplicantName; // replace with actual applicant name

                        }
                        else
                        {
                            applicantName = $"Inside Case - {history.Case.CaseNumber}";

                        }
                        string applicantFolder = Path.Combine(folderName, applicantName);



                        string pathToSave = Path.Combine(Directory.GetCurrentDirectory(), applicantFolder);

                        //Create directory if not exists
                        if (!Directory.Exists(pathToSave))
                            Directory.CreateDirectory(pathToSave);

                        if (file.Length > 0)
                        {
                            string fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                            string fullPath = Path.Combine(pathToSave, fileName);
                            string dbPath = Path.Combine(applicantFolder, fileName);

                            using (var stream = new FileStream(fullPath, FileMode.Create))
                            {
                                file.CopyTo(stream);
                            }
                            CaseAttachment attachment = new()
                            {
                                Id = Guid.NewGuid(),
                                CreatedAt = DateTime.Now,
                                CreatedBy = Guid.Parse(Request.Form["userId"]),
                                RowStatus = RowStatus.Active,
                                CaseId = history.CaseId,
                                FilePath = dbPath
                            };
                            caseCompeleteDto.CaseAttachments.Add(attachment);
                        }

                    }
                }

                await _caseProcessingService.CompleteTask(caseCompeleteDto);
                return NoContent();

            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpPost("revert")]
        public async Task<IActionResult> RevertCase(CaseRevertDto caseRevertDto)
        {
            try
            {
                await _caseProcessingService.RevertTask(caseRevertDto);
                return NoContent();

            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpPost("transfer"), DisableRequestSizeLimit]
        public async Task<IActionResult> TransferCase()
        {
            try
            {
                //public Guid CaseHistoryId { get; set; }
                //public Guid ToEmployeeId { get; set; }
                //public Guid FromEmployeeId { get; set; }
                //public Guid CaseTypeId { get; set; }
                //public Guid ToStructureId { get; set; }
                //public string Remark { get; set; }



                CaseTransferDto caseTransferDto = new()
                {
                    CaseHistoryId = Guid.Parse(Request.Form["CaseHistoryId"]),

                    //CaseTypeId = Guid.Parse(Request.Form["CaseTypeId"]),
                    FromEmployeeId = Guid.Parse(Request.Form["FromEmployeeId"]),
                    Remark = Request.Form["Remark"],
                    ToEmployeeId = Guid.Parse(Request.Form["ToEmployeeId"]),
                    ToStructureId = Guid.Parse(Request.Form["ToStructureId"])
                };
                var history = await _dbContext.CaseHistories.Where(x => x.Id == caseTransferDto.CaseHistoryId).Include(x => x.Case).FirstOrDefaultAsync();

                caseTransferDto.CaseAttachments = new List<CaseAttachment>();

                foreach (var file in Request.Form.Files)
                {

                    if (file.Name.ToLower() == "attachments")
                    {
                        string folderName = Path.Combine("Assets", "CaseAttachments");

                        string applicantName;

                        if (history.Case.ApplicantId != null)
                        {
                            var applicant =await _applicantService.GetApplicantById(history.Case.ApplicantId);
                            applicantName = applicant.Data.ApplicantName; // replace with actual applicant name

                        }
                        else
                        {
                            applicantName = $"Inside Case - {history.Case.CaseNumber}";

                        }
                        string applicantFolder = Path.Combine(folderName, applicantName);



                        string pathToSave = Path.Combine(Directory.GetCurrentDirectory(), applicantFolder);

                        //Create directory if not exists
                        if (!Directory.Exists(pathToSave))
                            Directory.CreateDirectory(pathToSave);

                        if (file.Length > 0)
                        {
                            string fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                            string fullPath = Path.Combine(pathToSave, fileName);
                            string dbPath = Path.Combine(applicantFolder, fileName);

                            using (var stream = new FileStream(fullPath, FileMode.Create))
                            {
                                file.CopyTo(stream);
                            }
                            CaseAttachment attachment = new()
                            {
                                Id = Guid.NewGuid(),
                                CreatedAt = DateTime.Now,
                                CreatedBy = Guid.Parse(Request.Form["userId"]),
                                RowStatus = RowStatus.Active,
                                CaseId = history.CaseId,
                                FilePath = dbPath
                            };
                            caseTransferDto.CaseAttachments.Add(attachment);
                        }

                    }
                }



                await _caseProcessingService.TransferCase(caseTransferDto);
                return NoContent();

            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpPost("waiting")]
        public async Task<IActionResult> AddToWaiting(Guid caseHistoryId)
        {
            try
            {

                return Ok(await _caseProcessingService.AddToWaiting(caseHistoryId));

            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error");
            }

        }

        [HttpPost("sendSms")]
        public async Task<IActionResult> SendSMS(CaseCompleteDto caseCompleteDto)
        {
            try
            {
                await _caseProcessingService.SendSMS(caseCompleteDto);
                return NoContent();

            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error");
            }

        }

        [HttpPost("archive")]
        public async Task<IActionResult> Archive(ArchivedCaseDto archivedCaseDto)
        {
            try
            {
                await _caseProcessingService.ArchiveCase(archivedCaseDto);
                return NoContent();

            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error");
            }

        }

        [HttpGet("GetCaseState")]
        public async Task<IActionResult> GetCaseState(Guid CaseTypeId, Guid caseHistoryId)
        {
            try
            {
                ;
                return Ok(await _caseProcessingService.GetCaseState(CaseTypeId, caseHistoryId));

            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error");
            }

        }

        [HttpGet("Ispermitted")]
        public async Task<IActionResult> Ispermitted(Guid employeeId, Guid caseId)
        {
            try
            {

                return Ok(await _caseProcessingService.Ispermitted(employeeId, caseId));

            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error");
            }

        }






    }
}
