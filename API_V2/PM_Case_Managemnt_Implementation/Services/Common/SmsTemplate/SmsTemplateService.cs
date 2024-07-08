using Microsoft.EntityFrameworkCore;
using PM_Case_Managemnt_Implementation.DTOS.Common;
using PM_Case_Managemnt_Implementation.Helpers;
using PM_Case_Managemnt_Implementation.Helpers.Logger;
using PM_Case_Managemnt_Implementation.Helpers.Response;
using PM_Case_Managemnt_Infrustructure.Data;
using PM_Case_Managemnt_Infrustructure.Models.Common;

namespace PM_Case_Managemnt_Implementation.Services.Common.SmsTemplate
{
    public class SmsTemplateService : ISmsTemplateService
    {
        private readonly ApplicationDbContext _dBContext;
        private readonly ILoggerManagerService _logger;

        public SmsTemplateService(ApplicationDbContext dBContext, ILoggerManagerService logger)
        {
            _dBContext = dBContext;
            _logger = logger;
        }

        public async Task<ResponseMessage<List<SmsTemplateGetDto>>> GetSmsTemplates(Guid subOrgId)
        {
            var response = new ResponseMessage<List<SmsTemplateGetDto>>();
            
            var templates = await _dBContext.SmsTemplates.Where(x => x.SubsidiaryOrganizationId == subOrgId).Select(x => new SmsTemplateGetDto
            {
                Id = x.Id,
                Title = x.Title,
                Description= x.Description,
                CreatedAt= x.CreatedAt,
                CreatedBy= x.CreatedBy,
                Remark= x.Remark,

            }).ToListAsync();

            response.Message = "Operation Successful.";
            response.Data = templates;
            response.Success = true;
            
            return response;
        }
        public async Task<ResponseMessage<SmsTemplateGetDto>> GetSmsTemplatebyId(Guid id)
        {
            var response = new ResponseMessage<SmsTemplateGetDto>();
            
            var template = await _dBContext.SmsTemplates.Where(x => x.Id == id).Select(x => new SmsTemplateGetDto
            {
                Id = x.Id,
                Title = x.Title,
                Description = x.Description,
                CreatedAt = x.CreatedAt,
                CreatedBy = x.CreatedBy,
                Remark = x.Remark,

            }).FirstOrDefaultAsync();

            
            response.Message = "Operation Successful.";
            response.Data = template;
            response.Success = true;
            
            return response;
        }

        public async Task<ResponseMessage<List<SelectListDto>>> GetSmsTemplateSelectList(Guid subOrgId)
        {
            var response = new ResponseMessage<List<SelectListDto>>();
            var templates = await _dBContext.SmsTemplates.Where(x => x.RowStatus == RowStatus.Active && x.SubsidiaryOrganizationId == subOrgId).Select(x => new SelectListDto
            {
                Id = x.Id,
                Name = x.Title,
                Photo = x.Description

            }).ToListAsync();

            response.Message = "Operation Successful.";
            response.Data = templates;
            response.Success = true;
            
            return response;
        }


        public async Task<ResponseMessage> CreateSmsTemplate(SmsTemplatePostDto smsTemplate)
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
                _logger.LogCreate("SmsTemplateService", smsTemplate.CreatedBy.ToString(), "SMS template created Successfully");
                return new ResponseMessage
                {
                    Success = true,
                    Message = "SMS Template Created Successfully"
                };

            }
            catch (Exception ex)
            {
                return new ResponseMessage
                {
                    Success = false,
                    Message = ex.Message
                };
            }

        }

        public async Task<ResponseMessage> UpdateSmsTemplate(SmsTemplateGetDto smsTemplate)
        {
            try
            {
                var template = await _dBContext.SmsTemplates.FirstOrDefaultAsync(x => x.Id == smsTemplate.Id);

                if (template is null)
                {
                    return new ResponseMessage
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
                    _logger.LogUpdate("SmsTemplateService", smsTemplate.CreatedBy.ToString(), "SMS Template updated Successfully");
                    return new ResponseMessage
                    {
                        Success = true,
                        Message = "SMS Template Updated Successfully"
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseMessage
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<ResponseMessage> DeleteSmsTemplate(Guid id)
        {
            var template = await _dBContext.SmsTemplates.FindAsync(id);
            if (template is null)
            {
                return new ResponseMessage
                {
                    Success = false,
                    Message = "SMS Template Not Found"
                };
            }
            else
            {
                _dBContext.SmsTemplates.Remove(template);
                await _dBContext.SaveChangesAsync();
                _logger.LogUpdate("SmsTemplateService", id.ToString(), "SMS Template deleted Successfully");
                return new ResponseMessage
                {
                    Success = true,
                    Message = "SMS Template Deleted Successfully"
                };
            }
        }



    }
}
