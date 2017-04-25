using Microsoft.AspNet.SignalR;

namespace SmtpMockWeb.WebLib
{
    public class MessageHub : Hub
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="link">eml link</param>
        /// <param name="link2">mht link, open by ie</param>
        public void Send(string message, string link, string link2)
        {
            Clients.All.broadcastMessage(message, link, link2);
        }
    }
}
