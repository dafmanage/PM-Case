using System.Net;
using Microsoft.EntityFrameworkCore;
using PM_Case_Managemnt_Implementation.Helpers.Logger;
using PM_Case_Managemnt_Implementation.Helpers.Response;
using PM_Case_Managemnt_Infrustructure.Data;
using PM_Case_Managemnt_Infrustructure.Models.CaseModel;

namespace PM_Case_Managemnt_Implementation.Services.CaseMGMT.CaseAttachments
{
    public class CaseAttachementService : ICaseAttachementService
    {
        private readonly ApplicationDbContext _dBContext;
        private readonly ILoggerManagerService _logger;
        public CaseAttachementService(ApplicationDbContext dBContext, ILoggerManagerService logger)
        {
            _dBContext = dBContext;
            _logger = logger;
        }

        public async Task<ResponseMessage<string>> AddMany(List<CaseAttachment> caseAttachments)
        {
            var response = new ResponseMessage<string>();
            try
            {
                await _dBContext.AddRangeAsync(caseAttachments);
                await _dBContext.SaveChangesAsync();
                response.Message = "Succesfully saved";
                response.Success = true;
                response.Data = "OK";
                //dindnt know who to put in created by part
                //_logger.LogCreate("CaseAttachementService", caseAttachments., "Applicant added Successfully");
                return response;
            } catch (Exception ex)
            {
                response.Message = "Error adding attachements";
                response.Success = false;
                response.Data = null;
                response.ErrorCode = HttpStatusCode.InternalServerError.ToString();
                return response;
            }
        }

        public async Task<ResponseMessage<List<CaseAttachment>>> GetAll(Guid subOrgId, string CaseId = null)
        {
            var response = new ResponseMessage<List<CaseAttachment>>();
            try
            {
                List<CaseAttachment> attachemnts = new List<CaseAttachment>();

                if (CaseId == null)
                    attachemnts = await _dBContext.CaseAttachments.Where(x => x.Case.SubsidiaryOrganizationId == subOrgId).ToListAsync();
                else
                    attachemnts = await _dBContext.CaseAttachments.Where(el => el.CaseId.Equals(Guid.Parse(CaseId))).ToListAsync();
                if (attachemnts == null){
                    response.Message = "Couldnt get any attachments with the given criteria.";
                    response.Success = false;
                    response.Data = null;
                    response.ErrorCode = HttpStatusCode.NotFound.ToString();
                    return response;
                }
                response.Message = "Attachments fetched successfully";
                response.Success = true;
                response.Data = attachemnts;
                return response;
            } catch(Exception ex) {
                response.Message = "Error Getting Attachments";
                response.Success = false;
                response.Data = null;
                response.ErrorCode = HttpStatusCode.InternalServerError.ToString();
                return response;
            }
        }

        public ResponseMessage<bool> RemoveAttachment(Guid attachmentId)
        {
            var response = new ResponseMessage<bool>();

            try
            {
                var case1 = _dBContext.CaseAttachments.Find(attachmentId);
                if (case1 == null){
                    response.Message = "could not find attachment with the given attachmentID";
                    response.Success = false;
                    response.ErrorCode = HttpStatusCode.NotFound.ToString();
                    response.Data = false;
                    return response;
                }
                _dBContext.CaseAttachments.Remove(case1!);
                _dBContext.SaveChanges();
                response.Success = true;
                response.Message = "Attachment removed successfully.";
                response.Data = true;
                _logger.LogUpdate("CaseAttachmentService", attachmentId.ToString(), "The specified attachment has been removed Successfully");
                return response;
            }
            catch (Exception ex)
            {
                response.Message = "Error while removing attachment";
                response.Success = false;
                response.ErrorCode = HttpStatusCode.InternalServerError.ToString();
                response.Data = false;
                return response;
            }
        }
    }
}
