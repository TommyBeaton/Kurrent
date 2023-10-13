using Lighthouse.Interfaces;
using Lighthouse.Utils;
using Microsoft.Extensions.Options;

namespace Lighthouse.Extensions;

public static class WebApplicationExtensions
{
    public static void AddDynamicWebHooks(this WebApplication app)
    {
        var lighthouseConfig = app.Services.GetRequiredService<IOptions<LighthouseConfig>>().Value;
        
        var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger("WebApplicationExtensions");
        
        if ((lighthouseConfig?.Webhooks == null || !lighthouseConfig.Webhooks.Any()) && 
            (lighthouseConfig?.Pollers == null || !lighthouseConfig.Pollers.Any()))
            throw new InvalidOperationException("No pollers or webhooks are not configured");
        
        foreach (var webhook in lighthouseConfig.Webhooks)
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