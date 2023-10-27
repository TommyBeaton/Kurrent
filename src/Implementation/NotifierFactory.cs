using Lighthouse.Interfaces;
using Lighthouse.Utils;

namespace Lighthouse.Implementation;

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
    
    public INotifier Create(Notifier notifier)
    {
        throw new NotImplementedException();
    }
}