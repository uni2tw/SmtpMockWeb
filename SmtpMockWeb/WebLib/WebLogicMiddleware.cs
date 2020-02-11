using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using nDumbster.SmtpMockServer;
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
                    DateTime.Now.ToString("HH:mm:ss"), msg), "");
                context.Response.Write("broadcast test");

            }
            if (context.Request.Path.Value == "/view")
            {
                string fid = context.Request.Query["fid"];
                string mailFilePath = Path.Combine(MailMessageWrapper.MailFolder, fid);
                if (File.Exists(mailFilePath))
                {
                    context.Response.Headers["Content-Type"] = "message/rfc822"; //"application/octet-stream";
                    context.Response.Headers["Content-Disposition"] =
                        "attachment; filename=\"" + fid + "\"";
                    context.Response.Write(File.ReadAllBytes(mailFilePath));
                }
                else
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                }
            }
            if (context.Request.Path.Value == "/mht")
            {
                string fid = context.Request.Query["fid"];
                string mailFilePath = Path.Combine(MailMessageWrapper.MailFolder, fid);
                if (File.Exists(mailFilePath))
                {
                    context.Response.Headers["Content-Type"] = "application/octet-stream";
                    context.Response.Headers["Content-Disposition"] =
                        "attachment; filename=\"" + fid + ".mht\"";
                    context.Response.Write(File.ReadAllBytes(mailFilePath));
                }
                else
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                }
            }
            if (context.Request.Path.Value == "/")
            {                
                if (context.Response.StatusCode == (int)HttpStatusCode.OK)
                {

                    context.Response.Headers["Content-Type"] = "text/html; charset=UTF-8";
                    context.Response.Write(
@"<html>
<head>

<script src='https://code.jquery.com/jquery-3.4.1.min.js'></script>
<script src='https://ajax.aspnetcdn.com/ajax/signalr/jquery.signalr-2.2.1.min.js'></script>
<script src='/signalr/hubs'></script>
<style>
.cell-text {
    display:inline-block;
    color:#5252e0;
}
.cell-link {
    display:inline-block;
}
.cell-icon {
    display:inline-block;
    margin-left:20px;
}
</style>
<script type='text/javascript'>
$(document).ready(function() {    
    function addMessage(msg, link, link2) {
        console.log(msg + ', ' + link + ', ' + link2);
        var elemLink = $('<a></a>').attr('href', link).attr('target','_blank');
        var elemLink2 = $('<a></a>').attr('href', link2).attr('target','_blank');
        var iconEml = $('<img />').attr('src','http://icons.iconarchive.com/icons/fatcow/farm-fresh/24/file-extension-eml-icon.png')
            .appendTo(elemLink);
        var iconMht = $('<img />').attr('src','http://icons.iconarchive.com/icons/tatice/cristal-intense/24/Internet-Explorer-icon.png')
            .appendTo(elemLink2);
        var cellText = $('<span></span>').addClass('cell-text').append(msg);
        var cellLink = $('<span></span>').addClass('cell-icon').append(elemLink);
        var cellLink2 = $('<span></span>').addClass('cell-icon').append(elemLink2);
        var messageRow = $('<div></div>').addClass('message')
            .append(cellText).append(cellLink).append(cellLink2);
        $('#container').prepend(messageRow);
    }
    var connection = $.hubConnection();
    var msgHub = connection.createHubProxy('MessageHub');
    msgHub.on('Send', function(message, link, link2) {
        addMessage(message, link, link2);
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