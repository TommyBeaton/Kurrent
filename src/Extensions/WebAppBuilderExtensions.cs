using Kurrent.Implementation;
using Kurrent.Implementation.Notifiers;
using Kurrent.Implementation.Pollers;
using Kurrent.Interfaces;
using Kurrent.Utils;

namespace Kurrent.Extensions;

public static class WebAppBuilderExtensions
{
    public static void RegisterApp(this WebApplicationBuilder builder, IConfiguration configuration)
    {
        if (builder.Environment.EnvironmentName.Contains("k8s"))
        {
            var reloadOnChange = Environment.GetEnvironmentVariable("ReloadConfigOnChange")?.ToLower() == "true";
            builder.Configuration.AddJsonFile(ConfigMapFileProvider.FromRelativePath("config"), "appsettings.k8s.json", optional: true, reloadOnChange: reloadOnChange);
        }

        #if DEBUG
        builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);
        #endif

        builder.Services.Configure<KurrentConfig>(
            configuration.GetSection("Kurrent")
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
                [KurrentStrings.Acr] = () => ctx.GetService<AcrPoller>(),
                [KurrentStrings.Docker] = () => ctx.GetService<DockerHubPoller>(),
            };
            var logger = ctx.GetService<ILogger<PollerFactory>>();
            return new PollerFactory(factories, logger);
        });

        builder.Services.AddSingleton<INotifierFactory>(ctx =>
        {
            var factories = new Dictionary<string, Func<INotifier>>()
            {
                [KurrentStrings.Slack] = () => ctx.GetService<SlackNotifier>(),
            };
            var logger = ctx.GetService<ILogger<NotifierFactory>>();
            return new NotifierFactory(factories, logger);
        });
        
        builder.Services.AddHostedService<PollerManager>();
    }
}