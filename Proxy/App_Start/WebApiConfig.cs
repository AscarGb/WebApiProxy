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
        string redirectLocation = WebConfigurationManager.AppSettings["ApiAdress"];
        string subApp = WebConfigurationManager.AppSettings["subApp"];

        static HttpClient _httpClient = new HttpClient();
        private async Task<HttpResponseMessage> RedirectRequest(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var localPath = request.RequestUri.LocalPath;

            var clonedRequest = await HttpRequestMessageExtensions.CloneHttpRequestMessageAsync(request);

            string url = redirectLocation + localPath.Replace(subApp, "") + request.RequestUri.Query;

            request.DisposeRequestResources();
            request.Dispose();

            request = clonedRequest;

            clonedRequest.RequestUri = new Uri(url);

            return await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        }
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
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