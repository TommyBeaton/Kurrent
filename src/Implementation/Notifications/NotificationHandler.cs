using Kurrent.Interfaces.Notifications;
using Kurrent.Models.Data;
using Kurrent.Utils;
using Microsoft.Extensions.Options;

namespace Kurrent.Implementation.Notifications;

public class NotificationHandler : INotificationHandler
{
    private readonly INotifierFactory _notifierFactory;
    private AppConfig _appConfig;
    private readonly ILogger<NotificationHandler> _logger;

    public NotificationHandler(
        INotifierFactory notifierFactory, 
        IOptionsMonitor<AppConfig> kurrentConfig,
        ILogger<NotificationHandler> logger)
    {
        _notifierFactory = notifierFactory;
        _appConfig = kurrentConfig.CurrentValue;
        _logger = logger;
        
        kurrentConfig.OnChange(config => _appConfig = config);
    }
    
    public async Task Send(Image image, RepositoryConfig repoConfig, string eventName, string? commitSha)
    {
        var notifierConfigs = _appConfig.Notifiers?.Where(x => x.EventSubscriptions.Contains(eventName));
        
        if (notifierConfigs == null || !notifierConfigs.Any())
        {
            _logger.LogWarning($"No Notifier configs found for event: {eventName}");
            return;
        }

        foreach (var notifierConfig in notifierConfigs)
        {
            var notifier = _notifierFactory.Create(notifierConfig.Type);
            if(notifier == null)
            {
                _logger.LogError($"Notifier not found for {notifierConfig.Type}");
                return;
            }
        
            await notifier.NotifyAsync(image, repoConfig, notifierConfig, commitSha);
        }
    }
}