namespace Kurrent.Interfaces.Notifications;

public interface INotifierFactory
{
    public INotifier? Create(string notifier);
}