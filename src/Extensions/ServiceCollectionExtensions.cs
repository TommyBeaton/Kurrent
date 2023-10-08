using Lighthouse.Implementation;
using Lighthouse.Implementation.Pollers;
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
        serviceCollection.AddTransient<IFileUpdater, FileUpdater>();
        serviceCollection.AddTransient<IGitService, GitService>();
        serviceCollection.AddTransient<AcrPoller>();
        serviceCollection.AddTransient<DockerHubPoller>();

        serviceCollection.AddSingleton<IPollerFactory>(ctx =>
        {
            var factories = new Dictionary<string, Func<IPoller>>()
            {
                [LighthouseStrings.Acr] = () => ctx.GetService<AcrPoller>(),
                [LighthouseStrings.Docker] = () => ctx.GetService<DockerHubPoller>(),
            };
            var logger = ctx.GetService<ILogger<PollerFactory>>();
            return new PollerFactory(factories, logger);
        });
        
        serviceCollection.AddHostedService<PollerManager>();
    }
}