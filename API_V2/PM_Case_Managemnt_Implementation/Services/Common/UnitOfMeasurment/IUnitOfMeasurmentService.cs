using PM_Case_Managemnt_Implementation.DTOS.Common;

namespace PM_Case_Managemnt_Implementation.Services.Common
{
    public interface IUnitOfMeasurmentService
    {

        public Task<int> CreateUnitOfMeasurment(UnitOfMeasurmentDto unitOfMeasurment);

        public Task<int> UpdateUnitOfMeasurment(UnitOfMeasurmentDto unitOfMeasurment);

        //public Task<int> UpdateOrganizationalProfile(OrganizationProfile organizationProfile);
        public Task<List<PM_Case_Managemnt_Infrustructure.Models.Common.UnitOfMeasurment>> GetUnitOfMeasurment(Guid subOrgId);

        public Task<List<SelectListDto>> getUnitOfMeasurmentSelectList(Guid subOrgId);



    }
}
