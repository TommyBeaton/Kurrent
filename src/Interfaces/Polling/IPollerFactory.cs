namespace Kurrent.Interfaces.Polling;

public interface IPollerFactory
{
    public IPoller Create(string type);
}