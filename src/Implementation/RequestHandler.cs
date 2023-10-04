using System.Text.Json;
using Lighthouse.Interfaces;
using Lighthouse.Models.Data;
using Lighthouse.Models.Data.Webhooks;
using Lighthouse.Utils;

namespace Lighthouse.Implementation;

public class RequestHandler : IRequestHandler
{
    private readonly ILogger<RequestHandler> _logger;

    public RequestHandler(ILogger<RequestHandler> logger)
    {
        _logger = logger;
    }
    
    public Container GetTagFromRequest(string requestBody, string webhookType)
    {
        Container container;
        switch (webhookType.ToLower())
        {
            case LighthouseStrings.Acr:
                container = GetTagFromAcrRequest(requestBody);
                break;
            case LighthouseStrings.Docker:
                container = GetTagFromDockerRequest(requestBody);
                break;
            default:
                _logger.LogError($"Webhook type {webhookType} not supported");
                return new Container();
        }

        if (!container.IsValid)
            return container;

        _logger.LogInformation($"Container image: {container} found in request body");
        
        return container;
    }

    private Container GetTagFromDockerRequest(string requestBody)
    {
        var dockerRequest = JsonSerializer.Deserialize<DockerHubEvent>(requestBody);
        
        if (dockerRequest?.Repository == null || dockerRequest.PushData == null)
            return LogAndReturnFailure(LighthouseStrings.Docker, requestBody);

        return new Container(Repository: dockerRequest.Repository.Name, Tag: dockerRequest.PushData.Tag);
    }

    private Container GetTagFromAcrRequest(string requestBody)
    {
        var acrRequest = JsonSerializer.Deserialize<AcrEvent>(requestBody);

        if (acrRequest?.Request == null || acrRequest.Target == null)
            return LogAndReturnFailure(LighthouseStrings.Acr, requestBody);
        
        return new Container(acrRequest.Request.Host, acrRequest.Target.Repository, acrRequest.Target.Tag);
    }
    
    private Container LogAndReturnFailure(string type, string requestBody)
    {
        _logger.LogError($"Could not deserialize {type} request. Request body: {requestBody}");
        return new Container();
    }
}