using Kurrent.Utils;

namespace Kurrent.Interfaces;

public interface INotifierFactory
{
    public INotifier? Create(string notifier);
}