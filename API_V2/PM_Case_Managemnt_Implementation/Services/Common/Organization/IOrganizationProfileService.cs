using PM_Case_Managemnt_API.DTOS.Common.Organization;
using PM_Case_Managemnt_Implementation.Helpers.Response;
using PM_Case_Managemnt_Infrustructure.Models.Common;

namespace PM_Case_Managemnt_Implementation.Services.Common
{
    public interface IOrganizationProfileService
    {
        public Task<ResponseMessage<int>> CreateOrganizationalProfile(OrganizationProfile organizationProfile);
        public Task<ResponseMessage<int>> UpdateOrganizationalProfile(OrganizationProfile organizationProfile);
        public Task<ResponseMessage<OrganizationProfileDto>> GetOrganizationProfile(Guid orgProId);
    }
}
