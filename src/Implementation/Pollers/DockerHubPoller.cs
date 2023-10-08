using System.Text.Json;
using Lighthouse.Interfaces;
using Lighthouse.Models.Data;
using Lighthouse.Models.Data.Pollers;
using Lighthouse.Utils;

namespace Lighthouse.Implementation.Pollers;

public class DockerHubPoller : BasePoller
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ISubscriptionHandler _subscriptionHandler;
    private readonly ILogger<DockerHubPoller> _logger;
    
    public DockerHubPoller(IHttpClientFactory httpClientFactory, ISubscriptionHandler subscriptionHandler, ILogger<DockerHubPoller> logger) : base(logger, LighthouseStrings.Docker)
    {
        _httpClientFactory = httpClientFactory;
        _subscriptionHandler = subscriptionHandler;
        _logger = logger;
    }
    
    protected override async void CheckForUpdates(object? state)
    {
        using var client = _httpClientFactory.CreateClient();
        client.BaseAddress = new Uri("https://registry.hub.docker.com/v2/");

        foreach (var image in Config.Images)
        {
            var httpResponse = await client.GetAsync($"repositories/library/{image}/tags");

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
            var response = JsonSerializer.Deserialize<DockerResponse>(jsonResponse);

            if (response == null)
            {
                _logger.LogWarning("Tried to parse ACR response in poller: {pollerName} but failed.",
                    Config);
                continue;
            }
            
            var sortedTags = response.Results.OrderByDescending(tag => tag.TagLastPushed).ToList();
            var latestTag = sortedTags.First().Name;
            
            if(LatestTag == latestTag)
                continue;

            LatestTag = latestTag;
            var container = new Container(Config.Url, image, latestTag);
            _subscriptionHandler.UpdateFromPoller(Config.Name, container);
        }

    }
}