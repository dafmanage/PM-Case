using Microsoft.AspNetCore.Mvc;
using PM_Case_Managemnt_Implementation.Services.CaseMGMT.CaseAttachments;

namespace PM_Case_Managemnt_API.Controllers.Case
{
    [Route("api/case")]
    [ApiController]
    public class CaseAttachementController : ControllerBase
    {
        private readonly ICaseAttachementService _caseAttachementService;

        public CaseAttachementController(ICaseAttachementService caseAttachementService)
        {
            _caseAttachementService = caseAttachementService;
        }

        [HttpGet("attachments")]
        public async Task<IActionResult> Get(Guid subOrgId, string CaseId = null)
        {
            try
            {
                return Ok(await _caseAttachementService.GetAll(subOrgId, CaseId));
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error");
            }
        }
        [HttpGet("removeAttachments")]

        public IActionResult RemoveAttachment(Guid attachmentId)
        {

            try
            {
                return Ok(_caseAttachementService.RemoveAttachment(attachmentId));
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error");
            }



        }


    }
}
