using System;
using System.IO;
using System.Reflection;
using Microsoft.Owin;
using SmtpMockWeb.Code;

namespace SmtpMockWeb.Owins
{
    public class StaticFilesMiddleware : OwinMiddlewareBase
    {
        public override void Execute(IOwinContext context)
        {
            string filePath = Path.Combine(Helper.GetCurrentDirectory(), Helper.ConvertToVirtualPath(context.Request.Path.Value));
            if (File.Exists(filePath))
            {
                context.Response.Write(File.ReadAllText(filePath));

            }            
        }

        public StaticFilesMiddleware(OwinMiddleware next) : base(next)
        {
        }
    }
}