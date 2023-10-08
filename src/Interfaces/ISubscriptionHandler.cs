using Lighthouse.Models.Data;

namespace Lighthouse.Interfaces;

public interface ISubscriptionHandler
{
    public void UpdateFromWebhook(string eventName, string type, string requestBody);
    public void UpdateFromPoller(string pollerName, Container container);
}