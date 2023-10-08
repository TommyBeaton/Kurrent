using Lighthouse.Interfaces;
using Lighthouse.Models.Data;
using Lighthouse.Utils;
using Microsoft.Extensions.Options;

namespace Lighthouse.Implementation;

public class SubscriptionHandler : ISubscriptionHandler
{
    private readonly IRequestHandler _requestHandler;
    private readonly IRepositoryUpdater _repositoryUpdater;
    private readonly LighthouseConfig _lighthouseConfig;
    private readonly ILogger<SubscriptionHandler> _logger;

    public SubscriptionHandler(
        IRequestHandler requestHandler,
        IRepositoryUpdater repositoryUpdater,
        IOptions<LighthouseConfig> lighthouseConfig,
        ILogger<SubscriptionHandler> logger)
    {
        _requestHandler = requestHandler;
        _repositoryUpdater = repositoryUpdater;
        _lighthouseConfig = lighthouseConfig.Value;
        _logger = logger;
    }
    
    public void UpdateFromWebhook(string eventName, string type, string requestBody)
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
        
        UpdateSubscribers(eventName, container);
    }

    public void UpdateFromPoller(string pollerName, Container container)
    {
        _logger.LogDebug("Received update from poller: {poller} with container: {container}",
            pollerName, 
            container);
        
        UpdateSubscribers(pollerName, container);
    }

    private void UpdateSubscribers(string eventName, Container container)
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
            _repositoryUpdater.UpdateAsync(
                subscriber.RepositoryName,
                container,
                subscriber.Branch
            );
        }
    }
}