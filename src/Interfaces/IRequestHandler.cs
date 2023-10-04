using Lighthouse.Models.Data;

namespace Lighthouse.Interfaces;

public interface IRequestHandler
{
    public Container GetTagFromRequest(string requestBody, string webhookType);
}