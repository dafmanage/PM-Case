using Microsoft.AspNetCore.Mvc;
using PM_Case_Managemnt_Implementation.DTOS.Case;
using PM_Case_Managemnt_Implementation.Services.CaseMGMT.CaseMessagesService;

namespace PM_Case_Managemnt_API.Controllers.Case
{
    [Route("api/case/[Controller]")]
    [ApiController]
    public class CaseMessagesController : ControllerBase
    {
        private readonly ICaseMessagesService _caseMessagesService;

        public CaseMessagesController(ICaseMessagesService caseMessagesService)
        {
            _caseMessagesService = caseMessagesService;
        }

        [HttpGet]
        public async Task<IActionResult> GetMany(Guid subOrgId, bool messageStatus = false)
        {
            try
            {
                return Ok(await _caseMessagesService.GetMany(subOrgId, messageStatus));
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> SendUnsentMessages([FromBody] List<CaseUnsentMessagesGetDto> messages)
        {
            try
            {
                await _caseMessagesService.SemdMessages(messages);
                return Ok();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }




    }
}
