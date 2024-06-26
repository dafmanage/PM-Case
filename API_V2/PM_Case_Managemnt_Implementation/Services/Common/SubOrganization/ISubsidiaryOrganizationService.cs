using PM_Case_Managemnt_Implementation.DTOS.Common;
using PM_Case_Managemnt_Implementation.DTOS.Common.Organization;
using PM_Case_Managemnt_Implementation.Helpers;

namespace PM_Case_Managemnt_Implementation.Services.Common.SubsidiaryOrganization
{
    public interface ISubsidiaryOrganizationService
    {

        Task<int> CreateSubsidiaryOrganization(SubOrgDto subOrg);
        Task<List<PM_Case_Managemnt_Infrustructure.Models.Common.Organization.SubsidiaryOrganization>> GetSubsidiaryOrganization();
        Task<ResponseMessage> UpdateSubsidiaryOrganization(SubOrgDto subsidiaryOrganization);
        Task<ResponseMessage> DeleteSubsidiaryOrganization(Guid suOrgId);
        Task<List<SelectListDto>> GetSubOrgSelectList();
        Task<PM_Case_Managemnt_Infrustructure.Models.Common.Organization.SubsidiaryOrganization> GetSubsidiaryOrganizationById(Guid subOrgId);

    }
}
