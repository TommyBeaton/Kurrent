using Kurrent.Models.Data;

namespace Kurrent.Interfaces;

public interface IFileUpdater
{
    public string TryUpdate(string content, Container container);
}