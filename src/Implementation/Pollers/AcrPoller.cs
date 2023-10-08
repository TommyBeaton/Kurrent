using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Lighthouse.Interfaces;
using Lighthouse.Models.Data;
using Lighthouse.Models.Data.Pollers;
using Lighthouse.Utils;

namespace Lighthouse.Implementation.Pollers;

public class AcrPoller : BasePoller
{
    private readonly ISubscriptionHandler _subscriptionHandler;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<AcrPoller> _logger;

    public AcrPoller(
        ISubscriptionHandler subscriptionHandler,
        IHttpClientFactory httpClientFactory,
        ILogger<AcrPoller> logger) : base(subscriptionHandler, httpClientFactory, logger, LighthouseStrings.Acr)
    {
        _subscriptionHandler = subscriptionHandler;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }
    
    protected override async void CheckForUpdates(object? state)
    {
        using var client = _httpClientFactory.CreateClient();
        var authenticationString = $"{Config!.Username}:{Config!.Password}";
        var b64AuthString = Convert.ToBase64String(
            Encoding.UTF8.GetBytes(authenticationString)
        );
        client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Basic", b64AuthString);
        client.BaseAddress = new Uri($"https://{Config!.Url}/acr/v1/");

        foreach (var image in Config.Images)
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
                    Config.Name);
                continue;
            }
            
            var jsonResponse = await httpResponse.Content.ReadAsStringAsync();
            var response = JsonSerializer.Deserialize<AcrResponse>(jsonResponse);

            if (response == null)
            {
                _logger.LogWarning("Tried to parse ACR response in poller: {pollerName} but failed.",
                    Config);
                continue;
            }

            var sortedTags = response.Tags.OrderByDescending(tag => tag.CreatedTime).ToList();
            var latestTag = sortedTags.First().Name;
            
            if(LatestTag == latestTag)
                continue;

            LatestTag = latestTag;
            var container = new Container(Config.Url, image, latestTag);
            _subscriptionHandler.UpdateFromPoller(Config.Name, container);
        }

    }
}