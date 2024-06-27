using Microsoft.EntityFrameworkCore;
using PM_Case_Managemnt_Infrustructure.Data;
using PM_Case_Managemnt_Infrustructure.Models.CaseModel;

namespace PM_Case_Managemnt_Implementation.Services.CaseMGMT.CaseAttachments
{
    public class CaseAttachementService : ICaseAttachementService
    {
        private readonly ApplicationDbContext _dBContext;

        public CaseAttachementService(ApplicationDbContext dBContext)
        {
            _dBContext = dBContext;
        }

        public async Task AddMany(List<CaseAttachment> caseAttachments)
        {
            try
            {
                await _dBContext.AddRangeAsync(caseAttachments);
                await _dBContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding attachemnts");
            }
        }

        public async Task<List<CaseAttachment>> GetAll(Guid subOrgId, string CaseId = null)
        {
            try
            {
                List<CaseAttachment> attachemnts = new List<CaseAttachment>();

                if (CaseId == null)
                    attachemnts = await _dBContext.CaseAttachments.Where(x => x.Case.SubsidiaryOrganizationId == subOrgId).ToListAsync();
                else
                    attachemnts = await _dBContext.CaseAttachments.Where(el => el.CaseId.Equals(Guid.Parse(CaseId))).ToListAsync();

                return attachemnts;
            }
            catch (Exception ex)
            {
                throw new Exception("Error Getting Attachments");
            }
        }

        public bool RemoveAttachment(Guid attachmentId)
        {

            try
            {
                var case1 = _dBContext.CaseAttachments.Find(attachmentId);

                _dBContext.CaseAttachments.Remove(case1!);
                _dBContext.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception();
            }
        }
    }
}
