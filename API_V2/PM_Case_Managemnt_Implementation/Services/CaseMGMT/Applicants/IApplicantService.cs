using PM_Case_Managemnt_Implementation.DTOS.CaseDto;
using PM_Case_Managemnt_Implementation.DTOS.Common;
using PM_Case_Managemnt_Infrustructure.Models.CaseModel;

namespace PM_Case_Managemnt_Implementation.Services.CaseMGMT.Applicants
{
    public interface IApplicantService
    {
        public Task<Guid> Add(ApplicantPostDto applicant);
        public Task<List<ApplicantGetDto>> GetAll(Guid subOrgId);

        public Task<List<SelectListDto>> GetSelectList(Guid subOrgId);
        Task<Guid> Update(ApplicantPostDto applicantPost);

        public Task<Applicant> GetApplicantById(Guid? applicantId);
    }
}
