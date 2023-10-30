using Kurrent.Models.Data;
using Kurrent.Models.Data.Notifiers;
using Kurrent.Utils;

namespace Kurrent.Interfaces;

public interface INotifier
{
    public Task<NotifierResult> NotifyAsync(Container container, RepositoryConfig repositoryConfig, NotifierConfig notifierConfig, string? commitSha);
}