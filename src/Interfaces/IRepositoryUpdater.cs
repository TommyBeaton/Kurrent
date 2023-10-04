using Lighthouse.Models.Data;

namespace Lighthouse.Interfaces;

public interface IRepositoryUpdater
{
    public Task UpdateAsync(string repositoryName, Container container, string branch = "main");
}