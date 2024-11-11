using Kurrent.Models.Data;
using Kurrent.Utils;

namespace Kurrent.Interfaces.Notifications;

public interface INotificationHandler
{
    public Task Send(Image image, RepositoryConfig repoConfig, string eventName, string? commitSha);
}