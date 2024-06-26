using Microsoft.AspNetCore.Mvc;
using PM_Case_Managemnt_Implementation.DTOS.CaseDto;
using PM_Case_Managemnt_Implementation.Services.CaseService.FileSettings;

namespace PM_Case_Managemnt_API.Controllers.Case
{
    [Route("api/case")]
    [ApiController]
    public class FileSettingController : ControllerBase
    {
        private readonly IFileSettingsService _fileSettingsService;

        public FileSettingController(IFileSettingsService fileSettingsService)
        {
            _fileSettingsService = fileSettingsService;
        }
        [HttpGet("fileSetting")]
        public async Task<IActionResult> GetAll(Guid subOrgId)
        {
            try
            {
                return Ok(await _fileSettingsService.GetAll(subOrgId));
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error");
            }
        }
        [HttpPut("fileSetting")]
        public async Task<IActionResult> UpdateFileSetting(FileSettingPostDto fileSettingPostDto)
        {
            try
            {
                return Ok(await _fileSettingsService.UpdateFilesetting(fileSettingPostDto));
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error");
            }
        }
        [HttpDelete("fileSetting")]
        public async Task<IActionResult> DeleteFileSetting(Guid fileId)
        {
            try
            {
                return Ok(await _fileSettingsService.DeleteFileSetting(fileId));
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpPost("fileSetting")]
        public async Task<IActionResult> PostFileSetting(FileSettingPostDto fileSettingPostDto)
        {
            try
            {
                await _fileSettingsService.Add(fileSettingPostDto);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error");
            }
        }
    }
}
