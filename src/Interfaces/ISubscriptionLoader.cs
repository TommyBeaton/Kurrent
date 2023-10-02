using Lighthouse.Models.Data;

namespace Lighthouse.Interfaces;

public interface ISubscriptionLoader
{
    IList<Subscription> Load();
}