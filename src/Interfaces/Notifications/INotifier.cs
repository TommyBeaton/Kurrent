using Kurrent.Models.Data;
using Kurrent.Models.Data.Notifiers;
using Kurrent.Utils;

namespace Kurrent.Interfaces.Notifications;

public interface INotifier
{
    public Task<NotifierResult> NotifyAsync(Image image, RepositoryConfig repositoryConfig, NotifierConfig notifierConfig, string? commitSha);
}