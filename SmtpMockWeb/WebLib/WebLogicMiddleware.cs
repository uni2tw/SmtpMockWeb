using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
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
            if (context.Request.Path.Value == "/send")
            {
                string msg = context.Request.Query["m"];
                var hub = GlobalHost.ConnectionManager.GetHubContext<MessageHub>();
                hub.Clients.All.Send(String.Format("{0} {1}",
                    DateTime.Now.ToString("HH:mm:ss"), msg));
                context.Response.Write("broadcast test");

            }
            if (context.Request.Path.Value == "/")
            {                
                if (context.Response.StatusCode == (int)HttpStatusCode.OK)
                {
                    context.Response.Headers["Content-Type"] = "text/html; charset=UTF-8";
                    context.Response.Write(
@"<html>
<head>
<script src='https://code.jquery.com/jquery-1.12.4.min.js' integrity='sha256-ZosEbRLbNQzLpnKIkEdrPv7lOy9C27hHQ+Xp8a4MxAQ=' crossorigin='anonymous'></script>
<script src='https://ajax.aspnetcdn.com/ajax/signalr/jquery.signalr-2.2.1.min.js'></script>
<script src='/signalr/hubs'></script>
<script type='text/javascript'>
$(document).ready(function() {    
    function addMessage(msg) {
        $('#container').prepend($('<div></div>').addClass('message').text(msg));
    }
    var connection = $.hubConnection();
    var msgHub = connection.createHubProxy('MessageHub');
    msgHub.on('Send', function(message) {
        addMessage(message);
    });
    connection.start()
        .done(function(){ 
            console.log('Now connected, connection ID=' + connection.id); })
        .fail(function(){ 
            console.log('Could not connect'); });

});
</script>
<style>
div.title {
  margin:0px;
  font-size:24px;
  font-weight:bold;
}
div.message {
  margin:8px;
  font-size:20px;
  border-bottom:1px dashed silver;
}
</style>
</head>
<body>
<div class='title'>" + Helper.GetProductVersion() + @" here</div>
<div id='container'>
</div>
</body>
</html>");
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