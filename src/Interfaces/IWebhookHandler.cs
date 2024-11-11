using Kurrent.Models.Data;
using Kurrent.Utils;

namespace Kurrent.Interfaces;

public interface IWebhookHandler
{
    public Task ProcessWebhook(HttpContext context, WebhookConfig webhook);
}