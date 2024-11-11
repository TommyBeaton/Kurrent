using Kurrent.Interfaces.Notifications;

namespace Kurrent.Implementation.Notifications;

public class NotifierFactory : INotifierFactory
{
    private readonly Dictionary<string, Func<INotifier>> _notifiers;
    private readonly ILogger<NotifierFactory> _logger;

    public NotifierFactory(
        Dictionary<string, Func<INotifier>> notifiers, 
        ILogger<NotifierFactory> logger)
    {
        _notifiers = notifiers;
        _logger = logger;
    }
    
    public INotifier? Create(string type)
    {
        _logger.LogTrace("Creating notifier of type {type}", type);
        
        if (!_notifiers.TryGetValue(type, out var factory) || factory == null)
            _logger.LogError("No notifier of type {type} found", type);

        return factory?.Invoke();
    }
}