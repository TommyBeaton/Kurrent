using System.Timers;
using Kurrent.Interfaces;
using Kurrent.Models.Data;
using Kurrent.Utils;

namespace Kurrent.Implementation.Pollers;

public abstract class BasePoller : IPoller
{
    private readonly ISubscriptionHandler _subscriptionHandler;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger _logger;
    
    private readonly string _type;
    private System.Timers.Timer _timer;

    protected PollerConfig? Config;
    
    private readonly Dictionary<string, string> _latestTags = new();
    private ElapsedEventHandler _handler;

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

    public void Start(PollerConfig config, CancellationToken token)
    {
        if (config.Type != _type)
        {
            _logger.LogWarning("Failed to start {type} poller. Poller type is not {type}. Poller config: {poller}",
                _type,
                _type,
                config);
        }

        _handler = (sender, e) => 
        {
            Task.Run(async () => await CheckForUpdates(token), token).Wait(token);
        };

        Config = config;
        _timer = new System.Timers.Timer(Config.IntervalInSeconds * 1000);
        _timer.Elapsed += _handler;
        _timer.AutoReset = true;
        _timer.Enabled = true;
        _timer.Start();
        CheckForUpdates(token);
    }

    public void Stop()
    {
        _timer.Stop();
        _timer.Elapsed -= _handler;
        _timer.Dispose();
    }

    private async Task CheckForUpdates(CancellationToken token)
    {
        foreach (var image in Config.Images)
        {
            if(token.IsCancellationRequested) //Check if we should stop
                Stop();
            
            _logger.LogInformation("Checking for {image} updates", image);
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
            httpResponse.Dispose();

            var latestTag = ExtractLatestTag(jsonResponse);

            if(string.IsNullOrEmpty(latestTag) || latestKnownTag == latestTag)
                continue;

            _latestTags[image] = latestTag;

            _logger.LogInformation("Found new tag {tag} for {image}", latestTag, image);
            
            var container = new Container(Config.Url, image, latestTag);
            _subscriptionHandler.UpdateFromPollerAsync(Config.EventName, container);
        }
    }

    protected abstract Task<HttpResponseMessage?> MakeHttpRequest(HttpClient client, string image);
    protected abstract string ExtractLatestTag(string httpResponse);
}