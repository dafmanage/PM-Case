using Microsoft.EntityFrameworkCore;
using PM_Case_Managemnt_Implementation.DTOS.Common;
using PM_Case_Managemnt_Implementation.Helpers.Response;
using PM_Case_Managemnt_Infrustructure.Data;
using PM_Case_Managemnt_Infrustructure.Models.Common;

namespace PM_Case_Managemnt_Implementation.Services.Common.SmsTemplate
{
    public class SmsTemplateService : ISmsTemplateService
    {
        private readonly ApplicationDbContext _dBContext;

        public SmsTemplateService(ApplicationDbContext dBContext)
        {
            _dBContext = dBContext;
        }

        public async Task<List<SmsTemplateGetDto>> GetSmsTemplates(Guid subOrgId)
        {
            var templates = await _dBContext.SmsTemplates.Where(x => x.SubsidiaryOrganizationId == subOrgId).Select(x => new SmsTemplateGetDto
            {
                Id = x.Id,
                Title = x.Title,
                Description = x.Description,
                CreatedAt = x.CreatedAt,
                CreatedBy = x.CreatedBy,
                Remark = x.Remark,

            }).ToListAsync();

            return templates;
        }

        public async Task<SmsTemplateGetDto> GetSmsTemplatebyId(Guid id)
        {
            var template = await _dBContext.SmsTemplates.Where(x => x.Id == id).Select(x => new SmsTemplateGetDto
            {
                Id = x.Id,
                Title = x.Title,
                Description = x.Description,
                CreatedAt = x.CreatedAt,
                CreatedBy = x.CreatedBy,
                Remark = x.Remark,

            }).FirstOrDefaultAsync();

            return template;
        }

        public async Task<List<SelectListDto>> GetSmsTemplateSelectList(Guid subOrgId)
        {
            var templates = await _dBContext.SmsTemplates.Where(x => x.RowStatus == RowStatus.Active && x.SubsidiaryOrganizationId == subOrgId).Select(x => new SelectListDto
            {
                Id = x.Id,
                Name = x.Title,
                Photo = x.Description

            }).ToListAsync();

            return templates;
        }

        public async Task<ResponseMessage<int>> CreateSmsTemplate(SmsTemplatePostDto smsTemplate)
        {
            try
            {
                PM_Case_Managemnt_Infrustructure.Models.Common.SmsTemplate template = new PM_Case_Managemnt_Infrustructure.Models.Common.SmsTemplate()
                {
                    Id = Guid.NewGuid(),
                    Title = smsTemplate.Title,
                    Description = smsTemplate.Description,
                    CreatedBy = smsTemplate.CreatedBy,
                    Remark = smsTemplate.Remark,
                    CreatedAt = DateTime.Now,
                    SubsidiaryOrganizationId = smsTemplate.SubsidiaryOrganizationId,
                };

                await _dBContext.SmsTemplates.AddAsync(template);
                await _dBContext.SaveChangesAsync();

                return new ResponseMessage<int>
                {
                    Success = true,
                    Message = "SMS Template Created Successfully"
                };

            }
            catch (Exception ex)
            {
                return new ResponseMessage<int>
                {
                    Success = false,
                    Message = ex.Message
                };
            }

        }

        public async Task<ResponseMessage<int>> UpdateSmsTemplate(SmsTemplateGetDto smsTemplate)
        {
            try
            {
                var template = await _dBContext.SmsTemplates.FirstOrDefaultAsync(x => x.Id == smsTemplate.Id);

                if (template is null)
                {
                    return new ResponseMessage<int>
                    {
                        Success = false,
                        Message = "SMS Template Not Found"
                    };
                }
                else
                {
                    template.Title = smsTemplate.Title;
                    template.Description = smsTemplate.Description;
                    template.Remark = smsTemplate.Remark;

                    //_dBContext.Entry(template).State = EntityState.Modified;
                    await _dBContext.SaveChangesAsync();

                    return new ResponseMessage<int>
                    {
                        Success = true,
                        Message = "SMS Template Updated Successfully"
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseMessage<int>
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<ResponseMessage<int>> DeleteSmsTemplate(Guid id)
        {
            var template = await _dBContext.SmsTemplates.FindAsync(id);
            if (template is null)
            {
                return new ResponseMessage<int>
                {
                    Success = false,
                    Message = "SMS Template Not Found"
                };
            }
            else
            {
                _dBContext.SmsTemplates.Remove(template);
                await _dBContext.SaveChangesAsync();
                return new ResponseMessage<int>
                {
                    Success = true,
                    Message = "SMS Template Deleted Successfully"
                };
            }
        }



    }
}
