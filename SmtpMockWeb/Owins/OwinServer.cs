using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Owin.Hosting;
using SmtpMockWeb.Code;

namespace SmtpMockWeb.Owins
{
    public interface IServer
    {
        void Start();
        void Stop();
        string StartUrl { get; set; }
    }

    public class OwinServer : IServer
    {
        public ICommonLog logger;
        private IDisposable myServer;

        public OwinServer()
        {
            logger = Ioc.Get<ICommonLog>();
        }

        public string StartUrl { get; set; }

        public void Start()
        {
            SystemConfig config = null;
            try
            {
                config = Ioc.Get<SystemConfig>();
            }
            catch (Exception ex)
            {
                logger.Log("讀取設定檔失敗，" + ex.ToString());
                Environment.Exit(-1);
            }
            List<string> ips = Helper.GetAllIps();
            //logger.Log("web server startup at port " + config.Port);
            StartOptions options = new StartOptions();

            if (Helper.IsUserAdministrator())
            {
                StartUrl = String.Format("http://{0}:{1}", config.RemoteIp, config.Port);
                options.Urls.Add(StartUrl);
            }
            else
            {
                StartUrl = "http://localhost:" + config.Port;
                options.Urls.Add(StartUrl);
            }
            logger.Log("web server start to listen " + StartUrl);
            try
            {
                myServer = WebApp.Start<Startup>(options);
            }
            catch (Exception ex)
            {
                File.WriteAllText(Path.Combine(Helper.GetCurrentDirectory(), "error.log"), ex.ToString());
                Environment.Exit(-1);
            }
        }

        public void Stop()
        {
            myServer.Dispose();
        }
    }
}