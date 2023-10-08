using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Lighthouse.Interfaces;
using Lighthouse.Models.Data;
using Lighthouse.Models.Data.Pollers;
using Lighthouse.Utils;

namespace Lighthouse.Implementation.Pollers;

public class AcrPoller : IPoller
{
    private readonly ISubscriptionHandler _subscriptionHandler;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<AcrPoller> _logger;

    private PollerConfig? _config;
    private Timer? _timer;
    
    private string _latestTag = string.Empty;
    
    public AcrPoller(
        ISubscriptionHandler subscriptionHandler,
        IHttpClientFactory httpClientFactory,
        ILogger<AcrPoller> logger)
    {
        _subscriptionHandler = subscriptionHandler;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public void Start(PollerConfig config)
    {
        if (config.Type != LighthouseStrings.Acr)
        {
            _logger.LogWarning("Failed to start ACR poller. Poller type is not ACR. Poller config: {poller}", config);
        }
        _config = config;
        _timer = new Timer(CheckForUpdates, null, 0, _config.IntervalInSeconds * 1000);
    }

    public void Stop()
    {
        _timer?.Dispose();
    }

    private async void CheckForUpdates(object? state)
    {
        using var client = _httpClientFactory.CreateClient();
        var authenticationString = $"{_config!.Username}:{_config!.Password}";
        var b64AuthString = Convert.ToBase64String(
            Encoding.UTF8.GetBytes(authenticationString)
        );
        client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Basic", b64AuthString);
        client.BaseAddress = new Uri($"https://{_config!.Url}/acr/v1/");

        foreach (var image in _config.Images)
        {
            var httpResponse = await client.GetAsync($"{image}/_tags");

            try
            {
                httpResponse.EnsureSuccessStatusCode();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to get tags for image {image} in poller: {pollerName}",
                    image,
                    _config.Name);
                continue;
            }
            
            var jsonResponse = await httpResponse.Content.ReadAsStringAsync();
            var response = JsonSerializer.Deserialize<AcrResponse>(jsonResponse);

            if (response == null)
            {
                _logger.LogWarning("Tried to parse ACR response in poller: {pollerName} but failed.",
                    _config);
                continue;
            }

            var sortedTags = response.Tags.OrderByDescending(tag => tag.CreatedTime).ToList();
            var latestTag = sortedTags.First().Name;
            
            if(latestTag == _latestTag)
                continue;
            
            var container = new Container(_config.Url, image, latestTag);
            _subscriptionHandler.UpdateFromPoller(_config.Name, container);
        }

    }
}