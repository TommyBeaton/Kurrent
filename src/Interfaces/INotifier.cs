using Lighthouse.Models.Data;
using Lighthouse.Utils;

namespace Lighthouse.Interfaces;

public interface INotifier
{
    public Task NotifyAsync(Container container, RepositoryConfig repositoryConfig, NotifierConfig notifierConfig);
}