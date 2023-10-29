using System;
using System.Net.Http;

namespace Lighthouse.IntegrationTest.Utils;

public class LighthouseHttpClientFactory : IHttpClientFactory, IDisposable
{
    private readonly Lazy<HttpMessageHandler> _handlerLazy = new (() => new HttpClientHandler());

    public HttpClient CreateClient(string name) => new (_handlerLazy.Value, disposeHandler: false);

    public void Dispose()
    {
        if (_handlerLazy.IsValueCreated)
        {
            _handlerLazy.Value.Dispose();
        }
    }
}