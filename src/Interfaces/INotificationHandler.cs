using Kurrent.Models.Data;
using Kurrent.Utils;

namespace Kurrent.Interfaces;

public interface INotificationHandler
{
    public Task Send(Container container, SubscriptionConfig subscriber, string? commitSha);
}