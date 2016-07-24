using Owin;
using SmtpMockWeb.Code;
using SmtpMockWeb.Owins;
using SmtpMockWeb.WebLib;

namespace SmtpMockWeb
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ICommonLog logger = Ioc.Get<ICommonLog>();
#if DEBUG
            logger.Log("owin init UseErrorPage.");
            app.UseErrorPage();
#endif
            logger.Log("owin init UseWelcomePage.");
            app.UseWelcomePage("/owin");

            logger.Log("owin init AuthenticateMiddleware.");
            //app.Use(typeof(AuthenticateMiddleware));
            //app.Use(typeof(LoggingMiddleware));
            app.Use(typeof(StaticFilesMiddleware));
            logger.Log("owin init WebLogicMiddleware.");
            app.Use(typeof(WebLogicMiddleware));

            //app.UseWebApi(new WebApiConfig());

            //app.MapSignalR();

            //app.Use(new Func<AppFunc, AppFunc>(ignoredNextApp => (AppFunc)Invoke));
        }
    }
}
