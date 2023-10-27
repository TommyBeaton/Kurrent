using Lighthouse.Models.Data;
using Lighthouse.Utils;

namespace Lighthouse.Interfaces;

public interface INotificationHandler
{
    public Task Send(Container container, RepositoryConfig repositoryConfig);
}