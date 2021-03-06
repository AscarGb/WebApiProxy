﻿using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Proxy
{
    public static class HttpRequestMessageExtensions
    {
        public static async Task<HttpRequestMessage> CloneHttpRequestMessageAsync(this HttpRequestMessage req)
        {
            HttpRequestMessage clone = null;
            MemoryStream ms = null;

            try
            {
                clone = new HttpRequestMessage(req.Method, req.RequestUri);
                ms = new MemoryStream();

                if (req.Content != null)
                {
                    await req.Content.CopyToAsync(ms);
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
            catch
            {
                clone?.Dispose();
                ms?.Dispose();
                throw;
            }
        }
    }
}