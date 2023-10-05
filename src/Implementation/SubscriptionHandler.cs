using Lighthouse.Interfaces;
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
    
    public async void Update(string webhookName, string type, string requestBody)
    {
        _logger.LogDebug($"Received webhook {webhookName} of type {type}");

        var container = _requestHandler.GetTagFromRequest(requestBody, type);

        if (!container.IsValid)
        {
            _logger.LogInformation($"Container image not found in request body for webhook {webhookName}. Cancelling update");
            return;
        }
        
        var subscribers =
            _lighthouseConfig.Subscriptions?.Where(x => x.WebhookName == webhookName).ToList();

        if (subscribers == null || !subscribers.Any())
        {
            _logger.LogWarning($"No subscriber found for webhook {webhookName}. Cancelling update.");
            return;
        }

        //Running the updates in parallel
        var updateTasks = subscribers.Select(subscriber => 
            _repositoryUpdater.UpdateAsync(subscriber.RepositoryName, container, subscriber.Branch)
        );
            
        await Task.WhenAll(updateTasks);
    }
}