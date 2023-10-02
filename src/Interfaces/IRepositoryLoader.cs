using Lighthouse.Models.Data;

namespace Lighthouse.Interfaces;

public interface IRepositoryLoader
{
    IList<Repository> Load();
}