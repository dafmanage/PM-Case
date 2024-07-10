using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using PM_Case_Managemnt_Implementation.Services.CaseMGMT.Applicants;
using PM_Case_Managemnt_Implementation.DTOS.CaseDto;
using PM_Case_Managemnt_Implementation.Helpers.Response;
using PM_Case_Managemnt_Implementation.Hubs.EncoderHub;
using PM_Case_Managemnt_Implementation.Services.CaseMGMT.CaseAttachments;
using PM_Case_Managemnt_Implementation.Services.CaseMGMT.FileInformationService;
using PM_Case_Managemnt_Implementation.Services.CaseService.Encode;
using PM_Case_Managemnt_Infrustructure.Models.Auth;
using PM_Case_Managemnt_Infrustructure.Models.CaseModel;
using PM_Case_Managemnt_Infrustructure.Models.Common;
using System.Net.Http.Headers;
using PM_Case_Managemnt_API.Services.CaseMGMT.Applicants;
using PM_Case_Managemnt_Implementation.Helpers.Response;

namespace PM_Case_Managemnt_API.Controllers.Case
{
    [Route("api/case")]
    [ApiController]
    public class CaseEncodingController : ControllerBase
    {
        private readonly ICaseEncodeService _caseEncodeService;
        private readonly ICaseAttachementService _caseAttachmentService;
        private readonly IFilesInformationService _filesInformationService;
        private readonly IApplicantServices _applicantService;
        private IHubContext<EncoderHub, IEncoderHubInterface> _encoderHub;
        private UserManager<ApplicationUser> _userManager;
        public CaseEncodingController(
            ICaseEncodeService caseEncodeService,
            ICaseAttachementService caseAttachementService,
            IFilesInformationService filesInformationService,
            IApplicantServices applicantService,
            IHubContext<EncoderHub, IEncoderHubInterface> encoderHub,
            UserManager<ApplicationUser> userManager)
        {
            _caseEncodeService = caseEncodeService;
            _caseAttachmentService = caseAttachementService;
            _filesInformationService = filesInformationService;
            _applicantService = applicantService;
            _encoderHub = encoderHub;
            _userManager = userManager;
        }


        [HttpPost("encoding"), DisableRequestSizeLimit]
        public async Task<IActionResult> Create()
        {
            try
            {
                CaseEncodePostDto caseEncodePostDto = new CaseEncodePostDto()
                {
                    CaseNumber = Request.Form["CaseNumber"],
                    LetterNumber = Request.Form["LetterNumber"],
                    LetterSubject = Request.Form["LetterSubject"],
                    CaseTypeId = Guid.Parse(Request.Form["CaseTypeId"]),
                    EmployeeId = !string.IsNullOrEmpty(Request.Form["EmployeeId"]) ? Guid.Parse(Request.Form["EmployeeId"]) : (Guid?)null,
                    ApplicantId = !string.IsNullOrEmpty(Request.Form["ApplicantId"]) ? Guid.Parse(Request.Form["ApplicantId"]) : (Guid?)null,
                    //ApplicantId = Guid.Parse(Request.Form["ApplicantId"]),
                    //EmployeeId = Guid.Parse(Request.Form["ApplicantId"]),
                    PhoneNumber2 = Request.Form["PhoneNumber2"],
                    Representative = Request.Form["Representative"],
                    CreatedBy = Guid.Parse(Request.Form["CreatedBy"]),
                    SubsidiaryOrganizationId = Guid.Parse(Request.Form["SubsidiaryOrganizationId"])
                };
                ResponseMessage<String> caseNO = await _caseEncodeService.Add(caseEncodePostDto);
                string caseId = caseNO.Data;
                var result = new { CaseId = caseId, CaseData = caseEncodePostDto };


                return Ok(result);


            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error");
            }

        }


        [HttpPost("fileEncoding"), DisableRequestSizeLimit]
        public async Task<IActionResult> Upload()
        {
            try
            {
                var caseId = Request.Form["CaseId"]/*.ToString().Split('_')[0]*/;
                var CaseMessage = await _caseEncodeService.GetSingleCase(Guid.Parse(caseId));
                var Case = CaseMessage.Data;
                var user = await _userManager.FindByIdAsync(Case.CreatedBy);
                //var employeeId = Request.Form["CaseId"].ToString().Split('_')[1];
                var employeeId = "";
                if (user != null)
                {
                    employeeId = user.EmployeesId.ToString();
                }

                if (Request.Form.Files.Any())
                {
                    List<CaseAttachment> attachments = new List<CaseAttachment>();
                    List<FilesInformation> fileInfos = new List<FilesInformation>();
                    List<CaseFilesGetDto> fileList = new List<CaseFilesGetDto>();
                    foreach (var file in Request.Form.Files)
                    {

                        if (file.Name.ToLower() == "attachments")
                        {
                            string folderName = Path.Combine("Assets", "CaseAttachments");

                            var applicant = await _applicantService.GetApplicantById(Guid.Parse(Case.ApplicantId));
                            string applicantName = applicant.Data.ApplicantName; // replace with actual applicant name
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
                                    CreatedBy = Guid.Parse(Case.Data.CreatedBy),
                                    RowStatus = RowStatus.Active,
                                    CaseId = Guid.Parse(caseId),
                                    FilePath = dbPath
                                };
                                attachments.Add(attachment);
                                CaseFilesGetDto uplodedFile = new()
                                {
                                    FileName = fileName,
                                    FilePath = dbPath
                                };
                                fileList.Add(uplodedFile);

                            }

                        }
                        else if (file.Name.ToLower() == "filesettings")
                        {
                            string folderName = Path.Combine("Assets", "FileSettings");
                            string pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);

                            // Create the directory if not exists
                            if (!Directory.Exists(pathToSave))
                                Directory.CreateDirectory(pathToSave);

                            if (file.Length > 0)
                            {
                                string fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                                string fullPath = Path.Combine(pathToSave, fileName);
                                string dbPath = Path.Combine(folderName, fileName);


                                using (var stream = new FileStream(fullPath, FileMode.Create))
                                {
                                    file.CopyTo(stream);
                                }

                                FilesInformation filesInformation = new()
                                {
                                    Id = Guid.NewGuid(),
                                    CreatedAt = DateTime.Now,
                                    CreatedBy = Guid.Parse(Case.Data.CreatedBy),
                                    RowStatus = RowStatus.Active,
                                    FilePath = dbPath,
                                    FileSettingId = Guid.Parse(fileName.Split(".")[0]),
                                    CaseId = Guid.Parse(caseId),
                                    filetype = file.ContentType
                                };
                                fileInfos.Add(filesInformation);


                            }
                        }



                    }
                    await _caseAttachmentService.AddMany(attachments);
                    await _filesInformationService.AddMany(fileInfos);
                    await _encoderHub.Clients.Group(employeeId).getUplodedFiles(fileList, employeeId);

                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error");
            }
        }



        [HttpPut("encoding"), DisableRequestSizeLimit]
        public async Task<IActionResult> Update()
        {
            try
            {
                CaseEncodePostDto caseEncodePostDto = new()
                {
                    CaseID = Guid.Parse(Request.Form["caseId"]),
                    CaseNumber = Request.Form["CaseNumber"],
                    LetterNumber = Request.Form["LetterNumber"],
                    LetterSubject = Request.Form["LetterSubject"],
                    CaseTypeId = Guid.Parse(Request.Form["CaseTypeId"]),
                    ApplicantId = Guid.Parse(Request.Form["ApplicantId"]),
                    EmployeeId = Guid.Parse(Request.Form["ApplicantId"]),
                    PhoneNumber2 = Request.Form["PhoneNumber2"],
                    Representative = Request.Form["Representative"],
                    CreatedBy = Guid.Parse(Request.Form["CreatedBy"]),
                };
                var caseIdMessage = await _caseEncodeService.Update(caseEncodePostDto);
                string caseId = caseIdMessage.Data;
                if (Request.Form.Files.Any())
                {
                    List<CaseAttachment> attachments = new List<CaseAttachment>();
                    List<FilesInformation> fileInfos = new List<FilesInformation>();
                    foreach (var file in Request.Form.Files)
                    {

                        if (file.Name.ToLower() == "attachments")
                        {
                            string folderName = Path.Combine("Assets", "CaseAttachments");

                            var applicant =await _applicantService.GetApplicantById(caseEncodePostDto.ApplicantId);
                            string applicantName = applicant.Data.ApplicantName; // replace with actual applicant name
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
                                    CreatedBy = caseEncodePostDto.CreatedBy,
                                    RowStatus = RowStatus.Active,
                                    CaseId = Guid.Parse(caseId.Data),
                                    FilePath = dbPath
                                };
                                attachments.Add(attachment);
                            }

                        }
                        else if (file.Name.ToLower() == "filesettings")
                        {
                            string folderName = Path.Combine("Assets", "FileSettings");
                            string pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);

                            // Create the directory if not exists
                            if (!Directory.Exists(pathToSave))
                                Directory.CreateDirectory(pathToSave);

                            if (file.Length > 0)
                            {
                                string fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                                string fullPath = Path.Combine(pathToSave, fileName);
                                string dbPath = Path.Combine(folderName, fileName);


                                using (var stream = new FileStream(fullPath, FileMode.Create))
                                {
                                    file.CopyTo(stream);
                                }

                                FilesInformation filesInformation = new()
                                {
                                    Id = Guid.NewGuid(),
                                    CreatedAt = DateTime.Now,
                                    CreatedBy = caseEncodePostDto.CreatedBy,
                                    RowStatus = RowStatus.Active,
                                    FilePath = dbPath,
                                    FileSettingId = Guid.Parse(fileName.Split(".")[0]),
                                    CaseId = Guid.Parse(caseId.Data),
                                    filetype = file.ContentType
                                };
                                fileInfos.Add(filesInformation);

                            }
                        }
                    }
                    await _caseAttachmentService.AddMany(attachments);
                    await _filesInformationService.AddMany(fileInfos);
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error");
            }

        }


        [HttpGet("encoding")]
        public async Task<IActionResult> GetAll(Guid userId)
        {
            try
            {
                return Ok(await _caseEncodeService.GetAll(userId));

            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error");
            }

        }
        [HttpGet("getCaseById")]
        public async Task<IActionResult> GetCaseById(Guid caseId)
        {
            try
            {
                return Ok(await _caseEncodeService.GetSingleCase(caseId));

            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error");
            }

        }


        [HttpGet("getCaseNumber")]

        public async Task<ResponseMessage<string>> GetCaseNumebr(Guid subOrgId)
        {

            return await _caseEncodeService.GetCaseNumber(subOrgId);

        }
        [HttpGet("getnotification")]
        public async Task<IActionResult> GetNotification(Guid employeeId)
        {
            try
            {
                return Ok(await _caseEncodeService.GetAllTransfred(employeeId));

            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error");
            }

        }
        [HttpGet("mycaseList")]
        public async Task<IActionResult> MyCaseList(Guid employeeId)
        {
            try
            {
                return Ok(await _caseEncodeService.MyCaseList(employeeId));

            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error");
            }

        }


        [HttpGet("searchCases")]
        public async Task<IActionResult> searchCases(string searchBY, Guid subOrgId)
        {
            try
            {
                return Ok(await _caseEncodeService.SearchCases(searchBY, subOrgId));

            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error");
            }

        }







        [HttpGet("completedList")]
        public async Task<IActionResult> CompletedCases(Guid subOrgId)
        {
            try
            {
                return Ok(await _caseEncodeService.CompletedCases(subOrgId));

            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error");
            }

        }
        [HttpGet("getArchivedCases")]

        public async Task<IActionResult> GetArchivedCases(Guid subOrgId)
        {
            try
            {
                return Ok(await _caseEncodeService.GetArchivedCases(subOrgId));

            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error");
            }

        }






    }
}
