using Lighthouse.Interfaces;
using Lighthouse.Models.Data;
using Lighthouse.Utils;

namespace Lighthouse.Implementation.Pollers;

public abstract class BasePoller : IPoller
{
    private readonly ISubscriptionHandler _subscriptionHandler;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger _logger;


    private readonly string _type;
    private Timer? _timer;

    protected PollerConfig? Config;
    protected string LatestTag = string.Empty;

    protected BasePoller(
        ISubscriptionHandler subscriptionHandler,
        IHttpClientFactory httpClientFactory,
        ILogger logger, 
        string type)
    {
        _subscriptionHandler = subscriptionHandler;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _type = type;
    }

    public void Start(PollerConfig config)
    {
        if (config.Type != _type)
        {
            _logger.LogWarning("Failed to start {type} poller. Poller type is not {type}. Poller config: {poller}",
                _type,
                _type,
                config);
        }

        Config = config;
        _timer = new Timer(CheckForUpdates, null, 0, Config.IntervalInSeconds * 1000);
    }

    public void Stop()
    {
        _timer?.Dispose();
    }

    protected virtual async void CheckForUpdates(object? state)
    {
        using var client = _httpClientFactory.CreateClient();

        foreach (var image in Config.Images)
        {
            var httpResponse = await MakeHttpRequest(client, image);

            if (httpResponse == null)
                continue;

            string jsonResponse = await httpResponse.Content.ReadAsStringAsync();
            var latestTag = ExtractLatestTag(jsonResponse, image);

            if(string.IsNullOrEmpty(latestTag) || LatestTag == latestTag)
                continue;

            LatestTag = latestTag;
            var container = new Container(Config.Url, image, latestTag);
            _subscriptionHandler.UpdateFromPoller(Config.Name, container);
        }
    }

    protected abstract Task<HttpResponseMessage?> MakeHttpRequest(HttpClient client, string image);
    protected abstract string ExtractLatestTag(string httpResponse, string image);
}