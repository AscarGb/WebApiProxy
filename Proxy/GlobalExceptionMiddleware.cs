using Microsoft.Owin;
using Proxy.Logging;
using System;
using System.Threading.Tasks;

namespace Proxy
{
    public class GlobalExceptionMiddleware : OwinMiddleware
    {
        private readonly ILog Logger = LogProvider.GetCurrentClassLogger();

        public GlobalExceptionMiddleware(OwinMiddleware next)
            : base(next)
        {
        }

        public override async Task Invoke(IOwinContext context)
        {
            try
            {
                await Next.Invoke(context);
            }
            catch (Exception ex)
            {
                Logger.ErrorException("Ошибка: ", ex);
            }
        }
    }
}