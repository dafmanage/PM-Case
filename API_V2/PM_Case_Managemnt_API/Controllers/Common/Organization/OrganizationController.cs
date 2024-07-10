
using Microsoft.AspNetCore.Mvc;
using PM_Case_Managemnt_Implementation.Services.Common;
using PM_Case_Managemnt_Infrustructure.Models.Common;
using System.Net.Http.Headers;
using PM_Case_Managemnt_API.DTOS.Common.Organization;

namespace PM_Case_Managemnt_API.Controllers.Common.Organization
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrganizationController : ControllerBase

    {
        private readonly IOrganizationProfileService _organizationProfileService;

        public OrganizationController(IOrganizationProfileService organzationProfileService)
        {
            _organizationProfileService = organzationProfileService;
        }

        [HttpPost, DisableRequestSizeLimit]

        public IActionResult Profile()
        {
            try
            {
                if (Request.Form.Files.Any())
                {
                    var file = Request.Form.Files[0];
                    var folderName = Path.Combine("Assets", "Organization");
                    var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);

                    if (!Directory.Exists(pathToSave))
                        Directory.CreateDirectory(pathToSave);

                    if (file.Length > 0)
                    {
                        var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                        var fullPath = Path.Combine(pathToSave, fileName);
                        var dbPath = Path.Combine(folderName, fileName);

                        using (var stream = new FileStream(fullPath, FileMode.Create))
                        {
                            file.CopyTo(stream);

                        }
                        var organizationalP = new OrganizationProfile
                        {
                            Id = Guid.NewGuid(),
                            Logo = dbPath,
                            OrganizationNameEnglish = Request.Form["organizationNameEnglish"],
                            OrganizationNameInLocalLanguage = Request.Form["organizationNameInLocalLanguage"],
                            Address = Request.Form["address"],
                            PhoneNumber = Request.Form["phoneNumber"],
                            Remark = Request.Form["remark"],
                            CreatedAt = DateTime.Now,
                            //SmsCode = Int32.Parse(Request.Form["SmsCode"]),
                            //UserName = Request.Form["UserName"],
                            //Password = Request.Form["Password"]
                        };


                        var response = _organizationProfileService.CreateOrganizationalProfile(organizationalP);


                        return Ok(new { response });
                    }

                    return BadRequest();

                }
                else
                {
                    return BadRequest();
                }
            }

            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error : {ex}");
            }
        }
        [HttpGet]

        public async Task<IActionResult> getProfile(Guid orgProId)
        {
            return Ok(await _organizationProfileService.GetOrganizationProfile(orgProId));
        }
        [HttpPut, DisableRequestSizeLimit]
        public IActionResult ProfileUpdate()
        {
            try
            {
                string dbpath = "";
                if (Request.Form.Files.Any())
                {
                    var file = Request.Form.Files[0];
                    var folderName = Path.Combine("Assets", "Organization");
                    var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);

                    if (!Directory.Exists(pathToSave))
                        Directory.CreateDirectory(pathToSave);

                    if (file.Length > 0)
                    {
                        var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                        var fileNameSave = Request.Form["id"] + "." + fileName.Split('.')[1];
                        var fullPath = Path.Combine(pathToSave, fileNameSave);
                        dbpath = Path.Combine(folderName, fileNameSave);

                        if (System.IO.File.Exists(fullPath))
                        {
                            System.IO.File.Delete(fullPath);
                        }

                        using (var stream = new FileStream(fullPath, FileMode.Create))
                        {
                            file.CopyTo(stream);
                        }
                    }

                }
                var organizationalP = new OrganizationProfile
                {
                    Id = Guid.Parse(Request.Form["id"]),
                    Logo = dbpath != "" ? dbpath : Request.Form["logo"],
                    OrganizationNameEnglish = Request.Form["organizationNameEnglish"],
                    OrganizationNameInLocalLanguage = Request.Form["organizationNameInLocalLanguage"],
                    Address = Request.Form["address"],
                    PhoneNumber = Request.Form["phoneNumber"],
                    Remark = Request.Form["remark"],
                    CreatedAt = DateTime.Now,
                    //SmsCode = Int32.Parse( Request.Form["SmsCode"]),
                    //UserName = Request.Form["UserName"],
                    //Password = Request.Form["Password"]
                };

                var response = _organizationProfileService.UpdateOrganizationalProfile(organizationalP);





                return Ok(new { response });

            }

            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error : {ex}");
            }
        }


    }

}