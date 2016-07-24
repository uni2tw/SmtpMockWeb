using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Owin;
using SmtpMockWeb.Code;

namespace SmtpMockWeb.WebLib
{
    public class WebLogicMiddleware : OwinMiddleware
    {
        private static ICommonLog logger = Ioc.Get<ICommonLog>();
        public WebLogicMiddleware(OwinMiddleware next)
            : base(next)
        {
        }

        public void BizLogicInvoke(IOwinContext context)
        {
            if (context.Request.Path.Value == "/")
            {
                
                if (context.Response.StatusCode == (int)HttpStatusCode.OK)
                {
                    context.Response.Headers["Content-Type"] = "text/html; charset=UTF-8";
                    context.Response.Write(String.Format(@"
<html>
<head>
</head>
<body>
<div class='title'>{0}</div>
<div id='container'>
</div>
</body>
</html>", Helper.GetProductVersion()));
                }
            }          
        }

        public override Task Invoke(IOwinContext context)
        {
            logger.Log("QueryFormMiddleware get url: " + ((Microsoft.Owin.OwinRequest)context.Request).Uri.ToString());
            try
            {
                BizLogicInvoke(context);
            }
            catch (Exception ex)
            {
                logger.Log(ex.Message);
            }
            return Next.Invoke(context);
        }
    }
   
    public class TemplateHelper
    {
        private static string _root;

        static TemplateHelper()
        {
            _root = Path.Combine(Helper.GetCurrentDirectory(), "_templates");
        }
        //"/Query.html"
        public static string Render(string reqFileName)
        {
            string filePath = Path.Combine(_root, reqFileName);
            if (File.Exists(filePath))
            {
                return File.ReadAllText(filePath);
            }
            return String.Empty;
        }
    }
}