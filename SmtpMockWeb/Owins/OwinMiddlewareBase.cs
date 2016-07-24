using System.Net;
using System.Threading.Tasks;
using Microsoft.Owin;

namespace SmtpMockWeb.Owins
{
    public abstract class OwinMiddlewareBase : OwinMiddleware
    {
        public OwinMiddlewareBase(OwinMiddleware next)
            : base(next)
        {
        }
       
        public override Task Invoke(IOwinContext context)
        {
            bool end = context.Response.StatusCode != 200;
            if (end == false)
            {
                Execute(context);
            }
            return Next.Invoke(context);            
        }

        protected void End(IOwinContext context, HttpStatusCode statusCode)
        {
            context.Response.StatusCode = (int)statusCode;
        }

        public abstract void Execute(IOwinContext context);
    }
}