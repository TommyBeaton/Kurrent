using System.Text.Json;
using Kurrent.Interfaces;
using Kurrent.Models.Data;
using Kurrent.Models.Data.Webhooks;
using Kurrent.Utils;

namespace Kurrent.Implementation;

public class WebhookHandler : IWebhookHandler
{
    private readonly ISubscriptionHandler _subscriptionHandler;
    private readonly ILogger<WebhookHandler> _logger;

    public WebhookHandler(
        ISubscriptionHandler subscriptionHandler, 
        ILogger<WebhookHandler> logger)
    {
        _subscriptionHandler = subscriptionHandler;
        _logger = logger;
    }
    
    public async Task ProcessRequestAsync(HttpContext context, WebhookConfig webhook)
    {
        using var reader = new StreamReader(context.Request.Body);
        var requestBody = await reader.ReadToEndAsync();

        _logger.LogDebug("Received payload for webhook {WebhookName}: {Payload}", webhook.EventName, requestBody);

        var image = GetTagFromRequest(requestBody, webhook.Type);

        if (image is null)
        {
            return;
        }
        
        await _subscriptionHandler.UpdateAsync(webhook.EventName, image);
    }
    
    private Image? GetTagFromRequest(string requestBody, string webhookType)
    {
        Image? image;
        switch (webhookType.ToLower())
        {
            case KurrentStrings.Acr:
                image = GetTagFromAcrRequest(requestBody);
                break;
            case KurrentStrings.Docker:
                image = GetTagFromDockerRequest(requestBody);
                break;
            default:
                _logger.LogError($"Webhook type {webhookType} not supported");
                return null;
        }

        if (image is null || !image.IsValid)
        {
            return null;
        }
        
        _logger.LogInformation($"Image: {image} found in request body");
        
        return image;
    }

    private Image? GetTagFromDockerRequest(string requestBody)
    {
        DockerHubEvent? dockerRequest = TryParseResponse<DockerHubEvent>(requestBody);

        if (dockerRequest?.Repository == null || dockerRequest.PushData == null)
        {
            LogFailure(KurrentStrings.Docker, requestBody);
            return null;
        }

        return new Image(Repository: dockerRequest.Repository.Name, Tag: dockerRequest.PushData.Tag);
    }

    private Image? GetTagFromAcrRequest(string requestBody)
    {
        AcrEvent? acrRequest = TryParseResponse<AcrEvent>(requestBody);

        if (acrRequest?.Request == null || acrRequest.Target == null)
        {
            LogFailure(KurrentStrings.Acr, requestBody);
            return null;
        }
        
        return new Image(acrRequest.Request.Host, acrRequest.Target.Repository, acrRequest.Target.Tag);
    }

    private T? TryParseResponse<T>(string requestBody)
    {
        T? response = default;
        try
        {
            response = JsonSerializer.Deserialize<T>(requestBody);
            return response;
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
        }

        return response;
    }
    
    private void LogFailure(string type, string requestBody)
    {
        _logger.LogError($"Could not deserialize {type} request. Request body: {requestBody}");
    }
}