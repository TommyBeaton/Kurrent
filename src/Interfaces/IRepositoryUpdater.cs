using Kurrent.Models.Data;

namespace Kurrent.Interfaces;

public interface IRepositoryUpdater
{
    /// <summary>
    /// Update an in a  repository with the given container.
    /// </summary>
    /// <param name="repositoryName">The name of the repository to update.</param>
    /// <param name="container">The container details</param>
    /// <param name="branch">The branch to commit changes to. defaults to main.</param>
    /// <returns>Returns true if the repository was updated and the commit sha</returns>
    public Task<(bool, string?)> UpdateAsync(string repositoryName, Container container, string branch = "main");
}