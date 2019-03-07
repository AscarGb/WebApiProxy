using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Configuration;

namespace Proxy
{
    public class ProxyHandler : DelegatingHandler
    {
        static int _instanceCount = 0;

        string redirectLocation = WebConfigurationManager.AppSettings["ApiAdress"];
        string subApp = WebConfigurationManager.AppSettings["subApp"];

        HttpClient _httpClient = new HttpClient { Timeout = Timeout.InfiniteTimeSpan };

        public ProxyHandler()
        {
            _instanceCount++;
            if (_instanceCount > 1)
            {
                throw new Exception("Только один экземпляр может быть создан");
            }
        }

        private async Task<HttpResponseMessage> RedirectRequest(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            HttpResponseMessage response = null;
            HttpRequestMessage clonedRequest = null;
            CancellationTokenSource tokenSource = null;
            try
            {
                tokenSource = new CancellationTokenSource();
                tokenSource.CancelAfter(Timeout.InfiniteTimeSpan);

                var localPath = request.RequestUri.LocalPath;

                clonedRequest = await request.CloneHttpRequestMessageAsync();

                string url = string.Format("{0}{1}{2}", redirectLocation, localPath.Replace(subApp, ""), request.RequestUri.Query);

                clonedRequest.RequestUri = new Uri(url);

                response = await _httpClient.SendAsync(clonedRequest, HttpCompletionOption.ResponseHeadersRead, tokenSource.Token);
                return response;
            }
            catch
            {
                clonedRequest?.Dispose();
                response?.Dispose();
                tokenSource?.Dispose();
                throw;
            }
            finally
            {
                request?.Dispose();
            }
        }
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            return RedirectRequest(request, cancellationToken);
        }

        private bool _disposed = false;
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (!_disposed)
                {
                    _httpClient?.Dispose();
                    _disposed = true;
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }
    }
}