using Microsoft.AspNetCore.Mvc;
using PM_Case_Managemnt_Implementation.DTOS.Common.Archive;
using PM_Case_Managemnt_Implementation.Services.Common.ShelfService;

namespace PM_Case_Managemnt_API.Controllers.Common.Archive
{
    [Route("api/common/archive")]
    [ApiController]
    public class ShelfController : ControllerBase
    {
        private readonly IShelfService _shelfService;

        public ShelfController(IShelfService shelfService)
        {
            _shelfService = shelfService;
        }

        [HttpPost("shelf")]
        public async Task<IActionResult> Create(ShelfPostDto shelfPostDto)
        {
            try
            {
                await _shelfService.Add(shelfPostDto);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpGet("shelf")]
        public async Task<IActionResult> GetAll(Guid subOrgId)
        {
            try
            {
                return Ok(await _shelfService.GetAll(subOrgId));
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error");
            }
        }

    }
}
