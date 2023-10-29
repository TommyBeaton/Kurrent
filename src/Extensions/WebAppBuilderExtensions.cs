using System.Text.Json;
using Lighthouse.Implementation;
using Lighthouse.Implementation.Notifiers;
using Lighthouse.Implementation.Pollers;
using Lighthouse.Interfaces;
using Lighthouse.Utils;

namespace Lighthouse.Extensions;

public static class WebAppBuilderExtensions
{
    public static void RegisterApp(this  WebApplicationBuilder builder, IConfiguration configuration)
    {
        builder.Configuration.AddJsonFile("appsettings.k8s.json", optional: true, reloadOnChange: true);

        builder.Services.Configure<LighthouseConfig>(
            configuration.GetSection("Lighthouse")
        );
        
        builder.Services.AddTransient<ISubscriptionHandler, SubscriptionHandler>();
        builder.Services.AddTransient<IRequestHandler, RequestHandler>();
        builder.Services.AddTransient<IRepositoryUpdater, RepositoryUpdater>();
        builder.Services.AddTransient<IFileUpdater, FileUpdater>();
        builder.Services.AddTransient<IGitService, GitService>();
        builder.Services.AddTransient<AcrPoller>();
        builder.Services.AddTransient<DockerHubPoller>();

        builder.Services.AddTransient<INotificationHandler, NotificationHandler>();
        builder.Services.AddTransient<SlackNotifier>();

        builder.Services.AddSingleton<IPollerFactory>(ctx =>
        {
            var factories = new Dictionary<string, Func<IPoller>>()
            {
                [LighthouseStrings.Acr] = () => ctx.GetService<AcrPoller>(),
                [LighthouseStrings.Docker] = () => ctx.GetService<DockerHubPoller>(),
            };
            var logger = ctx.GetService<ILogger<PollerFactory>>();
            return new PollerFactory(factories, logger);
        });

        builder.Services.AddSingleton<INotifierFactory>(ctx =>
        {
            var factories = new Dictionary<string, Func<INotifier>>()
            {
                [LighthouseStrings.Slack] = () => ctx.GetService<SlackNotifier>(),
            };
            var logger = ctx.GetService<ILogger<NotifierFactory>>();
            return new NotifierFactory(factories, logger);
        });
        
        builder.Services.AddHostedService<PollerManager>();
    }
}