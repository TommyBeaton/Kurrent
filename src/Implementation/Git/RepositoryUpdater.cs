using Kurrent.Interfaces.Git;
using Kurrent.Models.Data;
using Kurrent.Utils;
using LibGit2Sharp;

namespace Kurrent.Implementation.Git;

public class RepositoryUpdater : IRepositoryUpdater
{
    private readonly IFileUpdater _fileUpdater;
    private readonly IGitService _gitService;
    private readonly ILogger<RepositoryUpdater> _logger;

    public RepositoryUpdater(
        IFileUpdater fileUpdater,
        IGitService gitService,
        ILogger<RepositoryUpdater> logger)
    {
        _fileUpdater = fileUpdater;
        _gitService = gitService;
        _logger = logger;
    }

    public async Task<(bool, string?)> UpdateAsync(RepositoryConfig repoConfig, Image image)
    {
        _logger.LogInformation("Initiating update for repository: {repositoryName}.", repoConfig.Name);

        using var clonedRepo = _gitService.CloneAndCheckout(repoConfig);
        if (clonedRepo == null)
        {
            _logger.LogError("Failed to clone repository: {repositoryName}. Update cancelled.", repoConfig.Name);
            return (false, string.Empty);
        }

        if (!_gitService.CheckoutBranch(clonedRepo, repoConfig.Branch))
        {
            _logger.LogError("Failed to checkout branch: {branch} in repository: {repositoryName}. Update cancelled.", repoConfig.Branch, repoConfig.Name);
            DeleteRepo(clonedRepo);
            return (false, string.Empty);
        }

        await _fileUpdater.UpdateFilesInRepository(clonedRepo, repoConfig, image);

        if (!_gitService.HasChanges(clonedRepo))
        {
            _logger.LogInformation($"No changes found in repo: {repoConfig.Name} for image: {image.Name}.");
            return (false, string.Empty);
        }
        
        _logger.LogInformation("Committing and pushing changes for repository: {repositoryName}.", repoConfig.Name);
        var commitSha = _gitService.CommitAndPushChanges(
            clonedRepo,
            repoConfig,
            $"Kurrent update: {image.Host}/{image.Repository}:{image.Tag}"
        );
        
        DeleteRepo(clonedRepo);
        return (true, commitSha);
    }

    private void DeleteRepo(Repository repo)
    {
        Directory.Delete(repo.Info.WorkingDirectory, true);
        _logger.LogDebug("Deleted local copy of repository from: {repositoryPath}.", repo.Info.WorkingDirectory);
    }
}