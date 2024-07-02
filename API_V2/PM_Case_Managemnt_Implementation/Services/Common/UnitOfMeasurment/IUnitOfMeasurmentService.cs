using PM_Case_Managemnt_Implementation.DTOS.Common;
using PM_Case_Managemnt_Implementation.Helpers.Response;

namespace PM_Case_Managemnt_Implementation.Services.Common
{
    public interface IUnitOfMeasurmentService
    {

        public Task<ResponseMessage<int>> CreateUnitOfMeasurment(UnitOfMeasurmentDto unitOfMeasurment);

        public Task<ResponseMessage<int>> UpdateUnitOfMeasurment(UnitOfMeasurmentDto unitOfMeasurment);

        //public Task<int> UpdateOrganizationalProfile(OrganizationProfile organizationProfile);
        public Task<ResponseMessage<List< PM_Case_Managemnt_Infrustructure.Models.Common.UnitOfMeasurment >>> GetUnitOfMeasurment(Guid subOrgId);

        public Task<ResponseMessage<List<SelectListDto>>> getUnitOfMeasurmentSelectList(Guid subOrgId);



    }
}
