using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Http;

namespace Proxy
{
    public class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.MessageHandlers.Add(new ProxyHandler());
            config.Routes.MapHttpRoute("abe", "{*path}");
        }
    }
    public class ProxyHandler : DelegatingHandler
    {
        private async Task<HttpResponseMessage> RedirectRequest(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var redirectLocation = WebConfigurationManager.AppSettings["ApiAdress"];
            var subApp = WebConfigurationManager.AppSettings["subApp"];
            var localPath = request.RequestUri.LocalPath;

            using (var client = new HttpClient())
            using (var clonedRequest = await HttpRequestMessageExtensions.CloneHttpRequestMessageAsync(request))
            {
                clonedRequest.RequestUri = new Uri(redirectLocation + localPath.Replace(subApp, ""));
                return await client.SendAsync(clonedRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            }
        }

        protected override
            Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            return RedirectRequest(request, cancellationToken);
        }
    }

    public static class HttpRequestMessageExtensions
    {
        public static async Task<HttpRequestMessage> CloneHttpRequestMessageAsync(HttpRequestMessage req)
        {
            var clone = new HttpRequestMessage(req.Method, req.RequestUri);

            using (var ms = new MemoryStream())
            {
                if (req.Content != null)
                {
                    await req.Content.CopyToAsync(ms).ConfigureAwait(false);
                    ms.Position = 0;

                    if ((ms.Length > 0 || req.Content.Headers.Any()) && clone.Method != HttpMethod.Get)
                    {
                        clone.Content = new StreamContent(ms);

                        if (req.Content.Headers != null)
                            foreach (var h in req.Content.Headers)
                                clone.Content.Headers.Add(h.Key, h.Value);
                    }
                }

                clone.Version = req.Version;

                foreach (var prop in req.Properties)
                    clone.Properties.Add(prop);

                foreach (var header in req.Headers)
                    clone.Headers.TryAddWithoutValidation(header.Key, header.Value);

                return clone;
            }
        }
    }
}