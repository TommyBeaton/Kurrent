using Lighthouse.Interfaces;

namespace Lighthouse.Implementation;

public class PollerFactory : IPollerFactory
{
    private readonly Dictionary<string, Func<IPoller>> _pollers;
    private readonly ILogger<PollerFactory> _logger;

    public PollerFactory(
        Dictionary<string, Func<IPoller>> pollers, 
        ILogger<PollerFactory> logger)
    {
        _pollers = pollers;
        _logger = logger;
    }
    
    public IPoller Create(string type)
    {
        _logger.LogTrace("Creating poller of type {type}", type);

        if (!_pollers.TryGetValue(type, out var factory) || factory is null)
            _logger.LogError("No poller of type {type} found", type);
        
        return factory();
    }
}