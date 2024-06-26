using Microsoft.AspNetCore.Mvc;

namespace PM_Case_Managemnt_API.Controllers.Common
{
    [Route("api/[controller]")]
    [ApiController]
    public class pdfController : ControllerBase
    {

        [HttpGet]
        public async Task<IActionResult> GetPdf(string path)
        {

            var fullpath = Path.Combine(Directory.GetCurrentDirectory(), path);

            var bytes = System.IO.File.ReadAllBytes(fullpath);
            return File(bytes, "application/pdf");
            //var folderName = Path.Combine("Assets", "CaseAttachments", "12.pdf");
            //var fullpath = Path.Combine(Directory.GetCurrentDirectory(), folderName);
            //var fileStream = new FileStream(fullpath, FileMode.Open, FileAccess.Read);
            //fileStream.ReadTimeout = 5000; // set the read timeout before opening the stream
            //var result = new FileStreamResult(fileStream, "application/pdf");

            ////var fileStream = new FileStream(fullpath, FileMode.Open, FileAccess.Read);
            ////var result = new FileStreamResult(fileStream, "application/pdf");
            //Response.Headers.Add("Access-Control-Allow-Origin", "*");
            //Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization");
            //return result;
        }
    }
}
