using Lighthouse.Models.Data;
using Lighthouse.Models.Data.Notifiers;
using Lighthouse.Utils;

namespace Lighthouse.Interfaces;

public interface INotifier
{
    public Task<NotifierResult> NotifyAsync(Container container, RepositoryConfig repositoryConfig, NotifierConfig notifierConfig, string? commitSha);
}