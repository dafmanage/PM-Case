using PM_Case_Managemnt_Implementation.DTOS.CaseDto;
using PM_Case_Managemnt_Implementation.DTOS.Common;
using PM_Case_Managemnt_Implementation.Helpers.Response;

namespace PM_Case_Managemnt_Implementation.Services.CaseMGMT.Applicants
{
    public interface IApplicantService
    {
        public Task<ResponseMessage<Guid>> Add(ApplicantPostDto applicant);
        public Task<ResponseMessage<List<ApplicantGetDto>>> GetAll(Guid subOrgId);
        public Task<ResponseMessage<List<SelectListDto>>> GetSelectList(Guid subOrgId);
        public Task<ResponseMessage<Guid>> Update(ApplicantPostDto applicantPost);
        public Task<ResponseMessage<ApplicantGetDto>> GetApplicantById(Guid? applicantId);
    }
}
