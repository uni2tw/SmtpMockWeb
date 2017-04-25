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
            StartUrl = String.Format("http://localhost:{0}", config.Port);
            if (Helper.IsUserAdministrator())
            {
                options.Urls.Add("http://*:" + config.Port);
            }
            else
            {
                options.Urls.Add("http://localhost:" + config.Port);
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