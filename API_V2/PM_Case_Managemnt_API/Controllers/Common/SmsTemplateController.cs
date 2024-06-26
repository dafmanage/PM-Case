using Microsoft.AspNetCore.Mvc;
using PM_Case_Managemnt_Implementation.DTOS.Common;
using PM_Case_Managemnt_Implementation.Helpers;
using PM_Case_Managemnt_Implementation.Services.Common.SmsTemplate;
using System.Net;

namespace PM_Case_Managemnt_API.Controllers.Common
{
    [Route("api/[controller]")]
    [ApiController]
    public class SmsTemplateController : ControllerBase
    {
        private readonly ISmsTemplateService smsTemplateService;

        public SmsTemplateController(ISmsTemplateService smsTemplateService)
        {
            this.smsTemplateService = smsTemplateService;
        }

        [HttpGet("GetSmsTemplate")]
        [ProducesResponseType(typeof(SmsTemplateGetDto), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> Get(Guid subOrgId)
        {
            return Ok(await smsTemplateService.GetSmsTemplates(subOrgId));
        }

        [HttpGet("GetSmsTemplateById")]
        [ProducesResponseType(typeof(SmsTemplateGetDto), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetById(Guid id)
        {
            return Ok(await smsTemplateService.GetSmsTemplatebyId(id));
        }

        [HttpGet("GetSmsTemplateSelectList")]
        [ProducesResponseType(typeof(SelectListDto), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetSelectList(Guid subOrgId)
        {
            return Ok(await smsTemplateService.GetSmsTemplateSelectList(subOrgId));
        }

        [HttpPost("CreateSmsTemplate")]
        [ProducesResponseType(typeof(ResponseMessage), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> Create(SmsTemplatePostDto template)
        {
            if (ModelState.IsValid)
            {
                return Ok(await smsTemplateService.CreateSmsTemplate(template));
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpPut("UpdateSmsTemplate")]
        [ProducesResponseType(typeof(ResponseMessage), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> Update(SmsTemplateGetDto template)
        {
            if (ModelState.IsValid)
            {
                return Ok(await smsTemplateService.UpdateSmsTemplate(template));
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpDelete("DeleteSmsTemplate")]
        [ProducesResponseType(typeof(ResponseMessage), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> Delete(Guid id)
        {
            return Ok(await smsTemplateService.DeleteSmsTemplate(id));
        }


    }
}
