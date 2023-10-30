using Kurrent.Interfaces;
using Kurrent.Utils;
using Microsoft.Extensions.Options;

namespace Kurrent.Extensions;

public static class WebApplicationExtensions
{
    public static void AddDynamicWebHooks(this WebApplication app)
    {
        var kurrentConfig = app.Services.GetRequiredService<IOptions<KurrentConfig>>().Value;
        
        var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger("WebApplicationExtensions");
        
        if ((kurrentConfig?.Webhooks == null || !kurrentConfig.Webhooks.Any()) && 
            (kurrentConfig?.Pollers == null || !kurrentConfig.Pollers.Any()))
            throw new InvalidOperationException("No pollers or webhooks are not configured");
        
        if(kurrentConfig.Webhooks == null) return;
        
        foreach (var webhook in kurrentConfig.Webhooks)
        {
            logger.LogInformation("Adding webhook: {WebhookName} of type: {WebhookType} with path: {WebhookPath}", 
                webhook.EventName, webhook.Type, webhook.Path);

            app.MapPost(webhook.Path, async (HttpContext context, ISubscriptionHandler subscriptionHandler) =>
            {
                using var reader = new StreamReader(context.Request.Body);
                var requestBody = await reader.ReadToEndAsync();

                logger.LogDebug("Received payload for webhook {WebhookName}: {Payload}", webhook.EventName, requestBody);

                // Doesn't need to be awaited, there's no need to wait for the webhook to be processed
                subscriptionHandler.UpdateFromWebhook(webhook.EventName, webhook.Type, requestBody);
            }); 
        }
    }
}