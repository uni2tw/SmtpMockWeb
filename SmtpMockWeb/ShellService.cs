using System.ServiceProcess;
using SmtpMockWeb.Code;
using SmtpMockWeb.Owins;

namespace SmtpMockWeb
{
    partial class ShellService : ServiceBase
    {
        IServer server = Ioc.Get<IServer>();
        protected override void OnStart(string[] args)
        {
            
            server.Start();
        }

        protected override void OnStop()
        {
            Utility.SendNotifyStoppedMail();
            server.Stop();
        }

        public void Start()
        {
            OnStart(null);
        }
    }
}
