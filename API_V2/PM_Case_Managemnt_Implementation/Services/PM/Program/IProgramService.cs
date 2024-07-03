using PM_Case_Managemnt_Implementation.DTOS.Common;
using PM_Case_Managemnt_Implementation.DTOS.PM;
using PM_Case_Managemnt_Implementation.Helpers;
using PM_Case_Managemnt_Infrustructure.Models.PM;

namespace PM_Case_Managemnt_Implementation.Services.PM
{
    public interface IProgramService
    {
        public Task<int> CreateProgram(Programs Programs);
        public Task<List<ProgramDto>> GetPrograms(Guid subOrgId);
        public Task<List<SelectListDto>> GetProgramsSelectList(Guid subOrgId);
        public Task<ProgramDto> GetProgramsById(Guid programId);
        Task<ResponseMessage> UpdateProgram(ProgramPostDto program);
        Task<ResponseMessage> DeleteProgram(Guid programId);
    }
}
