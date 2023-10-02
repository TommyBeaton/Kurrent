using Lighthouse.Interfaces;
using Lighthouse.Models.Data;

namespace Lighthouse.Implementation;

public class RepositoryUpdater : IRepositoryUpdater
{
    private IList<Repository> _repositories;
    
    public RepositoryUpdater(IRepositoryLoader repositoryLoader)
    {
        _repositories = repositoryLoader.Load();
    }

    public void Update(string repositoryName)
    {
        throw new NotImplementedException();
    }
}