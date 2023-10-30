using Kurrent.Interfaces;
using Kurrent.Utils;
using Microsoft.Extensions.Options;

namespace Kurrent.Implementation;

public class PollerManager : IPollerManager
{
    private readonly IPollerFactory _pollerFactory;
    private readonly ILogger<PollerManager> _logger;
    private KurrentConfig _kurrentConfig;
    
    private readonly List<Task> _pollerTasks = new();

    public PollerManager(
        IOptionsMonitor<KurrentConfig> kurrentConfig, 
        IPollerFactory pollerFactory, 
        ILogger<PollerManager> logger)
    {
        _pollerFactory = pollerFactory;
        _logger = logger;
        _kurrentConfig = kurrentConfig.CurrentValue;
        kurrentConfig.OnChange(OnConfigChange);
    }

    private void Start()
    {
        if (_kurrentConfig?.Pollers == null)
        {
            _logger.LogWarning("No pollers found. This could be expected behaviour if none are configured.");
            return;
        }
        
        foreach (var pollerConfig in _kurrentConfig.Pollers)
        {
            var poller = _pollerFactory.Create(pollerConfig.Type);
            
            if(poller == null)
                throw new Exception($"Poller not found for {pollerConfig.Type}");
            
            _logger.LogInformation("Starting poller {poller}", pollerConfig.EventName);
            var task = Task.Run(() => poller.Start(pollerConfig));
            _pollerTasks.Add(task);
        }
    }

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting pollers");
        Start();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Stopping pollers");
        foreach(var task in _pollerTasks)
        {
            task.Dispose();
        }
        return Task.CompletedTask;
    }

    private async void OnConfigChange(KurrentConfig config)
    {
        _kurrentConfig = config;
        await StopAsync();
        await StartAsync();
    }
}