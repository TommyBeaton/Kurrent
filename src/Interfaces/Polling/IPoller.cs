using Kurrent.Utils;

namespace Kurrent.Interfaces.Polling;

public interface IPoller
{
    public void Start(PollerConfig config, CancellationToken cancellationToken);
    
    public void Stop();
}