using Lighthouse.Interfaces;
using Lighthouse.Models.Data;

namespace Lighthouse.Implementation;

public class SubscriptionHandler : ISubscriptionHandler
{
    private IList<Subscription> _subscriptions;
    
    public SubscriptionHandler(ISubscriptionLoader subscriptionLoader)
    {
        _subscriptions = subscriptionLoader.Load();
    }
    
    public void Update(string webhookName)
    {
        throw new NotImplementedException();
    }
}