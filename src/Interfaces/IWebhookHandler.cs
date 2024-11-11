using Kurrent.Utils;

namespace Kurrent.Interfaces;

public interface IWebhookHandler
{
    public Task ProcessRequestAsync(HttpContext context, WebhookConfig webhook);
}