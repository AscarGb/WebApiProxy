using System.Web.Http;
using System.Web.Http.ExceptionHandling;

namespace Proxy
{
    public class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.MessageHandlers.Add(new ProxyHandler());
            config.Routes.MapHttpRoute("all", "{*path}");

            config.Services.Replace(typeof(IExceptionHandler), new WebAPiExceptionHandler());
        }
    }    
}