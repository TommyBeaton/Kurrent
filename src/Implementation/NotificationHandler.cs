using Kurrent.Interfaces;
using Kurrent.Models.Data;
using Kurrent.Utils;
using Microsoft.Extensions.Options;

namespace Kurrent.Implementation;

public class NotificationHandler : INotificationHandler
{
    private readonly INotifierFactory _notifierFactory;
    private KurrentConfig _kurrentConfig;
    private readonly ILogger<NotificationHandler> _logger;

    public NotificationHandler(
        INotifierFactory notifierFactory, 
        IOptionsMonitor<KurrentConfig> kurrentConfig,
        ILogger<NotificationHandler> logger)
    {
        _notifierFactory = notifierFactory;
        _kurrentConfig = kurrentConfig.CurrentValue;
        _logger = logger;
        
        kurrentConfig.OnChange(config => _kurrentConfig = config);
    }
    
    public async Task Send(Container container, SubscriptionConfig subscriber, string? commitSha)
    {
        var repositoryConfig = _kurrentConfig.Repositories?.FirstOrDefault(x => x.Name == subscriber.RepositoryName);
        if (repositoryConfig == null)
        {
            _logger.LogError($"Repository config not found for {subscriber.RepositoryName}");
            return;
        }
        
        var notifierConfig = _kurrentConfig.Notifiers?.FirstOrDefault(x => x.EventName == subscriber.EventName);
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