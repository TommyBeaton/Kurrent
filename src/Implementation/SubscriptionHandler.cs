using Kurrent.Interfaces;
using Kurrent.Models.Data;
using Kurrent.Utils;
using Microsoft.Extensions.Options;

namespace Kurrent.Implementation;

public class SubscriptionHandler : ISubscriptionHandler
{
    private readonly IRequestHandler _requestHandler;
    private readonly IRepositoryUpdater _repositoryUpdater;
    private readonly INotificationHandler _notificationHandler;
    private KurrentConfig _kurrentConfig;
    private readonly ILogger<SubscriptionHandler> _logger;

    public SubscriptionHandler(
        IRequestHandler requestHandler,
        IRepositoryUpdater repositoryUpdater,
        IOptionsMonitor<KurrentConfig> kurrentConfig,
        INotificationHandler notificationHandler,
        ILogger<SubscriptionHandler> logger)
    {
        _requestHandler = requestHandler;
        _repositoryUpdater = repositoryUpdater;
        _notificationHandler = notificationHandler;
        _kurrentConfig = kurrentConfig.CurrentValue;
        _logger = logger;
        kurrentConfig.OnChange(config => _kurrentConfig = config);
    }
    
    public async Task UpdateFromWebhookAsync(string eventName, string type, string requestBody)
    {
        _logger.LogDebug("Received webhook {webhookName} of type {type}", 
            eventName, 
            type);

        var container = _requestHandler.GetTagFromRequest(requestBody, type);

        if (!container.IsValid)
        {
            _logger.LogInformation("Container image not found in request body for webhook {eventName}. Cancelling update", eventName);
            return;
        }
        
        await UpdateSubscribers(eventName, container);
    }

    public async Task UpdateFromPollerAsync(string pollerName, Container container)
    {
        _logger.LogDebug("Received update from poller: {poller} with container: {container}",
            pollerName, 
            container);
        
        await UpdateSubscribers(pollerName, container);
    }

    private async Task UpdateSubscribers(string eventName, Container container)
    {
        var subscribers =
            _kurrentConfig.Subscriptions?.Where(x => x.EventName == eventName).ToList();

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