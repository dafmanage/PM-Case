using Microsoft.AspNetCore.Mvc;
using PM_Case_Managemnt_Implementation.DTOS.Common.Archive;
using PM_Case_Managemnt_Implementation.Services.Common.RowService;

namespace PM_Case_Managemnt_API.Controllers.Common.Archive
{
    [Route("api/common/archive")]
    [ApiController]
    public class RowController : ControllerBase
    {
        private readonly IRowService _rowService;

        public RowController(IRowService rowService)
        {
            _rowService = rowService;
        }

        [HttpPost("row")]
        public async Task<IActionResult> Create(RowPostDto rowPostDto)
        {
            try
            {
                await _rowService.Add(rowPostDto);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpGet("row")]
        public async Task<IActionResult> GetAll(Guid shelfId)
        {
            try
            {
                return Ok(await _rowService.GetAll(shelfId));
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error");
            }
        }


    }
}
