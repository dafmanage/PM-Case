using PM_Case_Managemnt_Implementation.DTOS.CaseDto;

namespace PM_Case_Managemnt_Implementation.Hubs.EncoderHub
{
    public interface IEncoderHubInterface
    {
        Task getNotification(List<CaseEncodeGetDto> notifications, string employeeId);
        Task getUplodedFiles(List<CaseFilesGetDto> files, string employeeId);
        Task AddDirectorToGroup(string employeeId);

    }
}
