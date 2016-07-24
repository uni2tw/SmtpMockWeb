using Microsoft.AspNet.SignalR;

namespace SmtpMockWeb.WebLib
{
    public class MessageHub : Hub
    {
        public void Send(string message)
        {
            Clients.All.broadcastMessage(message);
        }
    }
}
