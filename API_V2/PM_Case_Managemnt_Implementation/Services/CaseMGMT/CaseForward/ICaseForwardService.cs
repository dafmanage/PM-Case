using PM_Case_Managemnt_Implementation.DTOS.CaseDto;

namespace PM_Case_Managemnt_Implementation.Services.CaseMGMT.CaseForwardService
{
    public interface ICaseForwardService
    {
        public Task AddMany(CaseForwardPostDto caseForwardPostDto);
    }
}
