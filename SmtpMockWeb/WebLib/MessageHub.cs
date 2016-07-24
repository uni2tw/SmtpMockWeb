using Microsoft.AspNet.SignalR;

namespace SmtpMockWeb.WebLib
{
    public class MessageHub : Hub
    {
        public void Send(string message, string link)
        {
            Clients.All.broadcastMessage(message, link);
        }
    }
}
