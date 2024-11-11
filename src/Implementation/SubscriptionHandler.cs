using Kurrent.Interfaces;
using Kurrent.Interfaces.Git;
using Kurrent.Interfaces.Notifications;
using Kurrent.Models.Data;
using Kurrent.Utils;
using Microsoft.Extensions.Options;

namespace Kurrent.Implementation;

public class SubscriptionHandler : ISubscriptionHandler
{
    private readonly IRepositoryUpdater _repositoryUpdater;
    private readonly INotificationHandler _notificationHandler;
    private AppConfig _appConfig;
    private readonly ILogger<SubscriptionHandler> _logger;

    public SubscriptionHandler(
        IRepositoryUpdater repositoryUpdater,
        IOptionsMonitor<AppConfig> kurrentConfig,
        INotificationHandler notificationHandler,
        ILogger<SubscriptionHandler> logger)
    {
        _repositoryUpdater = repositoryUpdater;
        _notificationHandler = notificationHandler;
        _appConfig = kurrentConfig.CurrentValue;
        _logger = logger;
        kurrentConfig.OnChange(config => _appConfig = config);
    }

    public async Task UpdateAsync(string eventName, Image image)
    {
        var repoConfigs =
            _appConfig.Repositories?.Where(x => x.EventSubscriptions.Contains(eventName)).ToList();

        if (repoConfigs == null || !repoConfigs.Any())
        {
            _logger.LogWarning($"No repo config found for event {eventName}. Cancelling update.");
            return;
        }

        foreach (var repoConfig in repoConfigs)
        {
             (bool didUpdate, string? commitSha) = await _repositoryUpdater.UpdateAsync(
                repoConfig,
                image
             );
             
             if(didUpdate)
                await _notificationHandler.Send(image, repoConfig, eventName, commitSha);
        }
    }
}