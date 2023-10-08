using Lighthouse.Utils;

namespace Lighthouse.Interfaces;

public interface IPollerFactory
{
    public IPoller Create(string type);
}