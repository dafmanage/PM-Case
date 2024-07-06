using PM_Case_Managemnt_Implementation.DTOS.Common;
using PM_Case_Managemnt_Implementation.Helpers.Response;

namespace PM_Case_Managemnt_Implementation.Services.Common.SmsTemplate
{
    public interface ISmsTemplateService
    {
        public Task<List<SmsTemplateGetDto>> GetSmsTemplates(Guid subOrgId);
        public Task<SmsTemplateGetDto> GetSmsTemplatebyId(Guid id);
        public Task<List<SelectListDto>> GetSmsTemplateSelectList(Guid subOrgId);
        public Task<ResponseMessage<int>> CreateSmsTemplate(SmsTemplatePostDto smsTemplate);
        public Task<ResponseMessage<int>> UpdateSmsTemplate(SmsTemplateGetDto smsTemplate);
        public Task<ResponseMessage<int>> DeleteSmsTemplate(Guid id);

    }
}
