using System.Text.Json;
using Kurrent.Interfaces;
using Kurrent.Models.Data;
using Kurrent.Models.Data.Webhooks;
using Kurrent.Utils;

namespace Kurrent.Implementation;

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
            case KurrentStrings.Acr:
                container = GetTagFromAcrRequest(requestBody);
                break;
            case KurrentStrings.Docker:
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
        DockerHubEvent? dockerRequest = TryParseResponse<DockerHubEvent>(requestBody);

        if (dockerRequest?.Repository == null || dockerRequest.PushData == null)
            return LogAndReturnFailure(KurrentStrings.Docker, requestBody);

        return new Container(Repository: dockerRequest.Repository.Name, Tag: dockerRequest.PushData.Tag);
    }

    private Container GetTagFromAcrRequest(string requestBody)
    {
        AcrEvent? acrRequest = TryParseResponse<AcrEvent>(requestBody);

        if (acrRequest?.Request == null || acrRequest.Target == null)
            return LogAndReturnFailure(KurrentStrings.Acr, requestBody);
        
        return new Container(acrRequest.Request.Host, acrRequest.Target.Repository, acrRequest.Target.Tag);
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
    
    private Container LogAndReturnFailure(string type, string requestBody)
    {
        _logger.LogError($"Could not deserialize {type} request. Request body: {requestBody}");
        return new Container();
    }
}