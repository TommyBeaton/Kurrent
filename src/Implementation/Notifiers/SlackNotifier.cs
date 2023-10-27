using System.Net.Http.Headers;
using Lighthouse.Interfaces;
using Lighthouse.Models.Data;
using Lighthouse.Models.Data.Notifiers;
using Lighthouse.Utils;

namespace Lighthouse.Implementation.Notifiers;

public class SlackNotifier : INotifier
{
    private const string Uri = "https://slack.com/api/chat.postMessage";
    
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<SlackNotifier> _logger;

    public SlackNotifier(
        IHttpClientFactory httpClientFactory, 
        ILogger<SlackNotifier> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }
    
    public async Task NotifyAsync(Container container, RepositoryConfig repositoryConfig, NotifierConfig notifierConfig)
    {
        using var client = _httpClientFactory.CreateClient();
        var message = new SlackMessage()
        {
            Channel = notifierConfig.Channel,
            Text = $"New image {container.ImageName} was updated in {repositoryConfig.Name} {LighthouseStrings.GetEmoji()}"
        };
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", notifierConfig.Token);
        
        try
        {
            var response = await client.PostAsJsonAsync(Uri, message);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while sending slack notification");
            return;
        }

        client.Dispose();
        
        _logger.LogInformation("Slack notification sent for container {container} in repository {repository}", 
            container.ImageName, 
            repositoryConfig.Name);
    }
}