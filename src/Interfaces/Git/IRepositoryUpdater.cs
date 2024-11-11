using Kurrent.Models.Data;
using Kurrent.Utils;

namespace Kurrent.Interfaces.Git;

public interface IRepositoryUpdater
{
    /// <summary>
    /// Update an image in a repository with the given container.
    /// </summary>
    /// <param name="repoConfig">The repository to update.</param>
    /// <param name="image">The container details</param>
    /// <returns>Returns true if the repository was updated and the commit sha</returns>
    public Task<(bool, string?)> UpdateAsync(RepositoryConfig repoConfig, Image image);
}