using System.Text.Json;
using Lighthouse.Interfaces;
using Lighthouse.Models.Data.Pollers;
using Lighthouse.Utils;

namespace Lighthouse.Implementation.Pollers;

public class DockerHubPoller : BasePoller
{
    private readonly ILogger<DockerHubPoller> _logger;
    
    public DockerHubPoller(
        ISubscriptionHandler subscriptionHandler, 
        IHttpClientFactory httpClientFactory,
        ILogger<DockerHubPoller> logger) 
        : base(
            subscriptionHandler, 
            httpClientFactory, 
            logger, 
            LighthouseStrings.Docker)
    {
        _logger = logger;
    }
    
    protected override async Task<HttpResponseMessage?> MakeHttpRequest(HttpClient client, string image)
    {
        client.BaseAddress = new Uri("https://registry.hub.docker.com/v2/");
        return await client.GetAsync($"repositories/library/{image}/tags");
    }

    protected override string ExtractLatestTag(string jsonResponse, string image)
    {
        var response = JsonSerializer.Deserialize<DockerResponse>(jsonResponse);
        if (response == null)
        {
            _logger.LogWarning("Failed to parse response in poller: {pollerName}", Config);
            return string.Empty;
        }

        var sortedTags = response.Results.OrderByDescending(tag => tag.TagLastPushed).ToList();
        return sortedTags.First().Name;
    }
}