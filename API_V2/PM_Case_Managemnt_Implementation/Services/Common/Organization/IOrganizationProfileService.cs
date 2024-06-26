using PM_Case_Managemnt_Infrustructure.Models.Common;

namespace PM_Case_Managemnt_Implementation.Services.Common
{
    public interface IOrganizationProfileService
    {
        public Task<int> CreateOrganizationalProfile(OrganizationProfile organizationProfile);
        public Task<int> UpdateOrganizationalProfile(OrganizationProfile organizationProfile);
        public Task<OrganizationProfile> GetOrganizationProfile(Guid orgProId);
    }
}
