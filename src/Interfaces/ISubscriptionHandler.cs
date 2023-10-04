namespace Lighthouse.Interfaces;

public interface ISubscriptionHandler
{
    public void Update(string webhookName, string type, string requestBody);
}