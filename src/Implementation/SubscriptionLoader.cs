using Lighthouse.Interfaces;
using Lighthouse.Models.Data;

namespace Lighthouse.Implementation;

public class SubscriptionLoader : ISubscriptionLoader
{
    private readonly IConfiguration _configuration;

    public SubscriptionLoader(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    public IList<Subscription> Load()
    {
        throw new NotImplementedException();
    }
}