using Kurrent.Models.Data;

namespace Kurrent.Interfaces;

public interface IRequestHandler
{
    public Container GetTagFromRequest(string requestBody, string webhookType);
}