using Microsoft.Owin;
using Owin;
using Proxy.App_Start;
using Serilog;
using System.Web.Hosting;
using System.Web.Http;

[assembly: OwinStartup(typeof(Startup))]
namespace Proxy.App_Start
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var log = new LoggerConfiguration()
               .WriteTo.File(HostingEnvironment.MapPath("~/logs/log.txt"), rollingInterval: RollingInterval.Day)
               .CreateLogger();
            Log.Logger = log;

            app.Use<GlobalExceptionMiddleware>();

            HttpConfiguration config = new HttpConfiguration();
            WebApiConfig.Register(config);  
            
            app.UseWebApi(config);
        }
    }
}