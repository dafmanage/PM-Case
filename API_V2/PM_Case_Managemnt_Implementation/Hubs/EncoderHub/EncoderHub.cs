using Microsoft.AspNetCore.SignalR;
using PM_Case_Managemnt_Implementation.DTOS.CaseDto;

namespace PM_Case_Managemnt_Implementation.Hubs.EncoderHub
{
    public class EncoderHub : Hub<IEncoderHubInterface>
    {

        public override async Task OnConnectedAsync()
        {

            await base.OnConnectedAsync();
        }

        public async Task AddDirectorToGroup(string employeeId)
        {
            var directorUserId = employeeId; // Replace with the actual director's user ID

            // Add the director to the specified group
            await Groups.AddToGroupAsync(Context.ConnectionId, directorUserId);

            // Call the client-side method 'getNotification' on all clients

        }

        public async Task getNotification(List<CaseEncodeGetDto> notifcations, string employeeId)
        {

            await Clients.Group(employeeId).getNotification(notifcations, employeeId);
        }
        public async Task getUplodedFiles(List<CaseFilesGetDto> files, string employeeId)
        {
            await Clients.Group(employeeId).getUplodedFiles(files, employeeId);
        }
    }
}
