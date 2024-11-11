using Kurrent.Models.Data;

namespace Kurrent.Interfaces;

public interface ISubscriptionHandler
{
    public Task UpdateAsync(string eventName, Image image);
}