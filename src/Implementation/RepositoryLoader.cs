using Lighthouse.Interfaces;
using Lighthouse.Models.Data;

namespace Lighthouse.Implementation;

public class RepositoryLoader : IRepositoryLoader
{
    private readonly IConfiguration _configuration;

    public RepositoryLoader(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    public IList<Repository> Load()
    {
        throw new NotImplementedException();
    }
}