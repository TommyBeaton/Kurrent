using Lighthouse.Interfaces;
using Lighthouse.Utils;
using Microsoft.Extensions.Options;

namespace Lighthouse.Implementation;

public class PollerManager : IPollerManager
{
    private readonly IPollerFactory _pollerFactory;
    private readonly ILogger<PollerManager> _logger;
    private readonly LighthouseConfig _lighthouseConfig;
    
    private readonly List<Task> _pollerTasks = new();

    public PollerManager(
        IOptions<LighthouseConfig> lighthouseConfig, 
        IPollerFactory pollerFactory, 
        ILogger<PollerManager> logger)
    {
        _pollerFactory = pollerFactory;
        _logger = logger;
        _lighthouseConfig = lighthouseConfig.Value;
    }

    private void Start()
    {
        if (_lighthouseConfig?.Pollers == null)
        {
            _logger.LogWarning("No pollers found. This could be expected behaviour if none are configured.");
            return;
        }
        
        foreach (var pollerConfig in _lighthouseConfig.Pollers)
        {
            var poller = _pollerFactory.Create(pollerConfig.Type);
            
            if(poller == null)
                throw new Exception($"Poller not found for {pollerConfig.Type}");
            
            _logger.LogInformation("Starting poller {poller}", pollerConfig.EventName);
            var task = Task.Run(() => poller.Start(pollerConfig));
            _pollerTasks.Add(task);
        }
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        Start();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        foreach(var task in _pollerTasks)
        {
            task.Dispose();
        }
        return Task.CompletedTask;
    }
}