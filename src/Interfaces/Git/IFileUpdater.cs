using Kurrent.Models.Data;
using Kurrent.Utils;
using LibGit2Sharp;

namespace Kurrent.Interfaces.Git;

public interface IFileUpdater
{
    public Task UpdateFilesInRepository(Repository repo, RepositoryConfig repoConfig, Image image);
}