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
    
    public async Task ProcessWebhook(HttpContext context, WebhookConfig webhook)
    {
        using var reader = new StreamReader(context.Request.Body);
        var requestBody = await reader.ReadToEndAsync();

        _logger.LogDebug("Received payload for webhook {WebhookName}: {Payload}", webhook.EventName, requestBody);

        var container = GetTagFromRequest(requestBody, webhook.Type);
        
        await _subscriptionHandler.UpdateAsync(webhook.EventName, container);
    }
    
    private Image GetTagFromRequest(string requestBody, string webhookType)
    {
        Image image;
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
                return new Image();
        }
        
        if (!image.IsValid)
            return image;

        _logger.LogInformation($"Container image: {image} found in request body");
        
        return image;
    }

    private Image GetTagFromDockerRequest(string requestBody)
    {
        DockerHubEvent? dockerRequest = TryParseResponse<DockerHubEvent>(requestBody);

        if (dockerRequest?.Repository == null || dockerRequest.PushData == null)
            return LogAndReturnFailure(KurrentStrings.Docker, requestBody);

        return new Image(Repository: dockerRequest.Repository.Name, Tag: dockerRequest.PushData.Tag);
    }

    private Image GetTagFromAcrRequest(string requestBody)
    {
        AcrEvent? acrRequest = TryParseResponse<AcrEvent>(requestBody);

        if (acrRequest?.Request == null || acrRequest.Target == null)
            return LogAndReturnFailure(KurrentStrings.Acr, requestBody);
        
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
    
    private Image LogAndReturnFailure(string type, string requestBody)
    {
        _logger.LogError($"Could not deserialize {type} request. Request body: {requestBody}");
        throw new JsonException($"Could not deserialize {type} request. Request body: {requestBody}");
    }
}