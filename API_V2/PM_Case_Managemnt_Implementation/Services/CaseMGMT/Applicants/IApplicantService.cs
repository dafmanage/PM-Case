﻿using PM_Case_Managemnt_Implementation.DTOS.CaseDto;
using PM_Case_Managemnt_Implementation.DTOS.Common;
using PM_Case_Managemnt_Infrustructure.Models.CaseModel;
using PM_Case_Managemnt_Implementation.Helpers.Response;

namespace PM_Case_Managemnt_API.Services.CaseMGMT.Applicants
{
    public interface IApplicantServices
    {
        public Task<ResponseMessage<Guid>> Add(ApplicantPostDto applicant);
        public Task<ResponseMessage<List<ApplicantGetDto>>> GetAll(Guid subOrgId);

        public Task<ResponseMessage<List<SelectListDto>>> GetSelectList(Guid subOrgId);
        public Task<ResponseMessage<Guid>> Update(ApplicantPostDto applicantPost);

        public Task<ResponseMessage<ApplicantGetDto>> GetApplicantById(Guid? applicantId);
    }
}
