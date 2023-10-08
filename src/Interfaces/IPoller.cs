using Lighthouse.Utils;

namespace Lighthouse.Interfaces;

public interface IPoller
{
    public void Start(PollerConfig config);
    
    public void Stop();
}