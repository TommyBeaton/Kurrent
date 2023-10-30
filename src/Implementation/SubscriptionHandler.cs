using Lighthouse.Interfaces;
using Lighthouse.Models.Data;
using Lighthouse.Utils;
using Microsoft.Extensions.Options;

namespace Lighthouse.Implementation;

public class SubscriptionHandler : ISubscriptionHandler
{
    private readonly IRequestHandler _requestHandler;
    private readonly IRepositoryUpdater _repositoryUpdater;
    private readonly INotificationHandler _notificationHandler;
    private LighthouseConfig _lighthouseConfig;
    private readonly ILogger<SubscriptionHandler> _logger;

    public SubscriptionHandler(
        IRequestHandler requestHandler,
        IRepositoryUpdater repositoryUpdater,
        IOptionsMonitor<LighthouseConfig> lighthouseConfig,
        INotificationHandler notificationHandler,
        ILogger<SubscriptionHandler> logger)
    {
        _requestHandler = requestHandler;
        _repositoryUpdater = repositoryUpdater;
        _notificationHandler = notificationHandler;
        _lighthouseConfig = lighthouseConfig.CurrentValue;
        _logger = logger;
        lighthouseConfig.OnChange(config => _lighthouseConfig = config);
    }
    
    public async void UpdateFromWebhook(string eventName, string type, string requestBody)
    {
        _logger.LogDebug("Received webhook {webhookName} of type {type}", 
            eventName, 
            type);

        var container = _requestHandler.GetTagFromRequest(requestBody, type);

        if (!container.IsValid)
        {
            _logger.LogInformation($"Container image not found in request body for webhook {eventName}. Cancelling update");
            return;
        }
        
        await UpdateSubscribers(eventName, container);
    }

    public async void UpdateFromPoller(string pollerName, Container container)
    {
        _logger.LogDebug("Received update from poller: {poller} with container: {container}",
            pollerName, 
            container);
        
        await UpdateSubscribers(pollerName, container);
    }

    private async Task UpdateSubscribers(string eventName, Container container)
    {
        var subscribers =
            _lighthouseConfig.Subscriptions?.Where(x => x.EventName == eventName).ToList();

        if (subscribers == null || !subscribers.Any())
        {
            _logger.LogWarning($"No subscriber found for event {eventName}. Cancelling update.");
            return;
        }

        foreach (var subscriber in subscribers)
        {
             (bool didUpdate, string? commitSha) = await _repositoryUpdater.UpdateAsync(
                subscriber.RepositoryName,
                container,
                subscriber.Branch
             );
             
             if(didUpdate)
                await _notificationHandler.Send(container, subscriber, commitSha);
        }
    }
}