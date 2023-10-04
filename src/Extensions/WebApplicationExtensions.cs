using Lighthouse.Interfaces;
using Lighthouse.Utils;
using Microsoft.Extensions.Options;

namespace Lighthouse.Extensions;

public static class WebApplicationExtensions
{
    public static void AddDynamicWebHooks(this WebApplication app)
    {
        var lighthouseConfig = app.Services.GetRequiredService<IOptions<LighthouseConfig>>().Value;

        if(lighthouseConfig?.Webhooks == null || !lighthouseConfig.Webhooks.Any())
            throw new InvalidOperationException("Webhooks are not configured");

        foreach (var webhook in lighthouseConfig.Webhooks)
        {
            app.MapPost(webhook.Path, async (HttpContext context, ISubscriptionHandler subscriptionHandler) =>
            {
                using var reader = new StreamReader(context.Request.Body);
                var requestBody = await reader.ReadToEndAsync();
                
                //Doesnt need to be awaited, there's no need to wait for the webhook to be processed
                subscriptionHandler.Update(webhook.Name, webhook.Type, requestBody);
            }); 
        }
    }
}