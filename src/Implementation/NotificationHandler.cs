using Lighthouse.Interfaces;
using Lighthouse.Models.Data;
using Lighthouse.Utils;
using Microsoft.Extensions.Options;

namespace Lighthouse.Implementation;

public class NotificationHandler : INotificationHandler
{
    private readonly INotifierFactory _notifierFactory;
    private LighthouseConfig _lighthouseConfig;
    private readonly ILogger<NotificationHandler> _logger;

    public NotificationHandler(
        INotifierFactory notifierFactory, 
        IOptionsMonitor<LighthouseConfig> lighthouseConfig,
        ILogger<NotificationHandler> logger)
    {
        _notifierFactory = notifierFactory;
        _lighthouseConfig = lighthouseConfig.CurrentValue;
        _logger = logger;
        
        lighthouseConfig.OnChange(config => _lighthouseConfig = config);
    }
    
    public async Task Send(Container container, SubscriptionConfig subscriber, string? commitSha)
    {
        var repositoryConfig = _lighthouseConfig.Repositories?.FirstOrDefault(x => x.Name == subscriber.RepositoryName);
        if (repositoryConfig == null)
        {
            _logger.LogError($"Repository config not found for {subscriber.RepositoryName}");
            return;
        }
        
        var notifierConfig = _lighthouseConfig.Notifiers?.FirstOrDefault(x => x.EventName == subscriber.EventName);
        if (notifierConfig == null)
        {
            _logger.LogError($"Notifier config not found for {subscriber.EventName}");
            return;
        }
        
        var notifier = _notifierFactory.Create(notifierConfig.Type);
        if(notifier == null)
        {
            _logger.LogError($"Notifier not found for {notifierConfig.Type}");
            return;
        }
        
        await notifier.NotifyAsync(container, repositoryConfig, notifierConfig, commitSha);
    }
}