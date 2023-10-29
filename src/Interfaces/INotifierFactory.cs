using Lighthouse.Utils;

namespace Lighthouse.Interfaces;

public interface INotifierFactory
{
    public INotifier? Create(string notifier);
}