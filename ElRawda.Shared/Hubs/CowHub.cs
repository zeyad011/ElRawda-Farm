using Microsoft.AspNetCore.SignalR;

namespace ElRawda.Hubs
{
    public class CowHub : Hub
    {
        public async Task SendMessage(string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", message);
        }
    }
}
