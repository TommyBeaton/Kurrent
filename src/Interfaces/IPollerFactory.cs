using Kurrent.Utils;

namespace Kurrent.Interfaces;

public interface IPollerFactory
{
    public IPoller Create(string type);
}