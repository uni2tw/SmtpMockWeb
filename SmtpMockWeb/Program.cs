using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using Microsoft.AspNet.SignalR;
using nDumbster.SmtpMockServer;
using SmtpMockWeb.Code;
using SmtpMockWeb.Owins;
using SmtpMockWeb.WebLib;

namespace SmtpMockWeb
{
    class Program
    {
        #region exit handle
        [DllImport("Kernel32")]
        private static extern bool SetConsoleCtrlHandler(EventHandler handler, bool add);

        private delegate bool EventHandler(CtrlType sig);
        static EventHandler _handler;

        enum CtrlType
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT = 1,
            CTRL_CLOSE_EVENT = 2,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT = 6
        }

        private static bool Handler(CtrlType sig)
        {
            switch (sig)
            {
                case CtrlType.CTRL_C_EVENT:
                case CtrlType.CTRL_LOGOFF_EVENT:
                case CtrlType.CTRL_SHUTDOWN_EVENT:
                case CtrlType.CTRL_CLOSE_EVENT:
                default:
                    Utility.SendNotifyStoppedMail();
                    return false;
            }
        }
        #endregion

        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            log4net.Config.XmlConfigurator.ConfigureAndWatch(new FileInfo(Path.Combine(Helper.GetCurrentDirectory(), "log4net.config")));
            Console.Title = Helper.GetProductVersion();
            Ioc.Init();
            _handler += new EventHandler(Handler);
            SetConsoleCtrlHandler(_handler, true);

            Utility.SendNotifyStartingMail();
            MainTask();

        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Ioc.Get<ICommonLog>().Log(e.ExceptionObject.ToString());
            Utility.SendNotifyTerminatedMail(e.ExceptionObject as Exception);
            Environment.Exit(0);
        }

        private static void MainTask()
        {
            IServer server = Ioc.Get<IServer>();
            SystemConfig config = Ioc.Get<SystemConfig>();

            if (Environment.UserInteractive == false)
            {
                var servicesToRun = new ServiceBase[]
                {
                    new ShellService()
                };
                // running as service
                ServiceBase.Run(servicesToRun);
            }
            else
            {
                MailMessageWrapper.Init(Path.Combine(Helper.GetCurrentDirectory(), "mail"));
                var smtpServer = SimpleSmtpServer.Start(
                    config.SmtpMockServer.HostIp,
                    config.SmtpMockServer.Port);
                smtpServer.OnMessageReceived += delegate (MailMessageWrapper mmp)
                {
                    var hub = GlobalHost.ConnectionManager.GetHubContext<MessageHub>();
                    hub.Clients.All.Send(mmp.ToString(), "/view?fid=" + mmp.FileName);
                };

                var service = new ShellService();
                service.Start();

                Process.Start(server.StartUrl);

                Console.WriteLine("Press Q key to stop...");

                ConsoleKey key;
                do
                {
                    key = Console.ReadKey(true).Key;
                } while (key != ConsoleKey.Q);
                service.Stop();

                smtpServer.Stop();
                Environment.Exit(0);
            }
        }
    }
}
