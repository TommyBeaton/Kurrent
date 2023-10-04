using Lighthouse.Implementation;
using Lighthouse.Interfaces;
using Lighthouse.Utils;

namespace Lighthouse.Extensions;

public static class ServiceCollectionExtensions
{
    public static void RegisterApp(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        serviceCollection.Configure<LighthouseConfig>(
            configuration.GetSection("Lighthouse")
        );
        serviceCollection.AddTransient<ISubscriptionHandler, SubscriptionHandler>();
        serviceCollection.AddTransient<IRequestHandler, RequestHandler>();
        serviceCollection.AddTransient<IRepositoryUpdater, RepositoryUpdater>();
    }
}