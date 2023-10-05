using Lighthouse.Models.Data;

namespace Lighthouse.Interfaces;

public interface IFileUpdater
{
    public string TryUpdate(string content, Container container);
}