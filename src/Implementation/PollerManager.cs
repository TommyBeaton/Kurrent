using Kurrent.Interfaces;
using Kurrent.Utils;
using Microsoft.Extensions.Options;

namespace Kurrent.Implementation;

public class PollerManager : IPollerManager
{
    private readonly IPollerFactory _pollerFactory;
    private readonly ILogger<PollerManager> _logger;
    private KurrentConfig _kurrentConfig;
    
    private CancellationTokenSource _cts;
    
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
        if (_kurrentConfig?.Pollers == null || !_kurrentConfig.Pollers.Any())
        {
            _logger.LogWarning("No pollers found. This could be expected behaviour if none are configured.");
            return;
        }

        _cts = new CancellationTokenSource();
        
        foreach (var pollerConfig in _kurrentConfig.Pollers)
        {
            var poller = _pollerFactory.Create(pollerConfig.Type);
            
            if(poller == null)
                throw new ArgumentOutOfRangeException(nameof(pollerConfig.Type), $"Poller not found for {pollerConfig.Type}");
            
            _logger.LogInformation("Starting poller {poller}", pollerConfig.EventName);
            var task = Task.Run(() => poller.Start(pollerConfig, _cts.Token));
            _pollerTasks.Add(task);
        }
    }

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting pollers");
        Start();
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Stopping pollers");
        
        // Signal all tasks to cancel
        _cts.Cancel();
        _cts.Dispose();

        try
        {
            // Wait for all tasks to complete
            await Task.WhenAll(_pollerTasks);
            _logger.LogInformation("All pollers stopped");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while stopping pollers");
        }
        
    }

    private async void OnConfigChange(KurrentConfig config)
    {
        _kurrentConfig = config;
        await StopAsync();
        await StartAsync();
    }
}