using Microsoft.AspNetCore.Mvc;
using PM_Case_Managemnt_Implementation.DTOS.Common.Archive;
using PM_Case_Managemnt_Implementation.Services.Common.FolderService;

namespace PM_Case_Managemnt_API.Controllers.Common.Archive
{
    [Route("api/common/archive")]
    [ApiController]
    public class FolderController : ControllerBase
    {
        private readonly IFolderService _folderService;

        public FolderController(IFolderService folderService)
        {
            _folderService = folderService;
        }

        [HttpPost("folder")]
        public async Task<IActionResult> Create(FolderPostDto folderPostDto)
        {
            try
            {
                await _folderService.Add(folderPostDto);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpGet("folder")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                return Ok(await _folderService.GetAll());
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error");
            }
        }


        [HttpGet("folder/filtered")]
        public async Task<IActionResult> GetFiltered(Guid? shelfId = null, Guid? rowId = null)
        {
            try
            {
                return Ok(await _folderService.GetFilltered(shelfId, rowId));
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }


    }
}
