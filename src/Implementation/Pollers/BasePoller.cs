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
    
    private readonly Dictionary<string, string> _latestTags = new();

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

    private async void CheckForUpdates(object? state)
    {
        foreach (var image in Config.Images)
        {
            if(!_latestTags.TryGetValue(image, out string latestKnownTag))
            {
                latestKnownTag = String.Empty;
                _latestTags.Add(image, latestKnownTag);
            }
            
            using var client = _httpClientFactory.CreateClient();
            var httpResponse = await MakeHttpRequest(client, image);

            if (httpResponse == null)
                continue;

            string jsonResponse = await httpResponse.Content.ReadAsStringAsync();
            client.Dispose();
            
            var latestTag = ExtractLatestTag(jsonResponse, image);

            if(string.IsNullOrEmpty(latestTag) || latestKnownTag == latestTag)
                continue;

            _latestTags[image] = latestTag;
            var container = new Container(Config.Url, image, latestTag);
            _subscriptionHandler.UpdateFromPoller(Config.EventName, container);
        }
    }

    protected abstract Task<HttpResponseMessage?> MakeHttpRequest(HttpClient client, string image);
    protected abstract string ExtractLatestTag(string httpResponse, string image);
}