using Kurrent.Models.Data;

namespace Kurrent.Interfaces;

public interface ISubscriptionHandler
{
    public Task UpdateFromWebhookAsync(string eventName, string type, string requestBody);
    public Task UpdateFromPollerAsync(string pollerName, Container container);
}