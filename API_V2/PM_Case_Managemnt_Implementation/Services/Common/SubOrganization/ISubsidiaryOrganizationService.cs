﻿using PM_Case_Managemnt_Implementation.DTOS.Common;
using PM_Case_Managemnt_Implementation.DTOS.Common.Organization;
using PM_Case_Managemnt_Implementation.Helpers;
using PM_Case_Managemnt_Implementation.Helpers.Response;

namespace PM_Case_Managemnt_Implementation.Services.Common.SubsidiaryOrganization
{
    public interface ISubsidiaryOrganizationService
    {

        Task<ResponseMessage<int>> CreateSubsidiaryOrganization(SubOrgDto subOrg);
        Task<ResponseMessage<List<PM_Case_Managemnt_Infrustructure.Models.Common.Organization.SubsidiaryOrganization>>> GetSubsidiaryOrganization();
        Task<ResponseMessage> UpdateSubsidiaryOrganization(SubOrgDto subsidiaryOrganization);
        Task<ResponseMessage> DeleteSubsidiaryOrganization(Guid suOrgId);
        Task<ResponseMessage<List<SelectListDto>>> GetSubOrgSelectList();
        Task<ResponseMessage<PM_Case_Managemnt_Infrustructure.Models.Common.Organization.SubsidiaryOrganization>> GetSubsidiaryOrganizationById(Guid subOrgId);

    }
}
