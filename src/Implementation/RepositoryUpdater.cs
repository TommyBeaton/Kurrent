using LibGit2Sharp;
using Lighthouse.Interfaces;
using Lighthouse.Models.Data;
using Lighthouse.Utils;
using Microsoft.Extensions.Options;

namespace Lighthouse.Implementation;

public class RepositoryUpdater : IRepositoryUpdater
{
    private LighthouseConfig _lighthouseConfig;
    private readonly IFileUpdater _fileUpdater;
    private readonly IGitService _gitService;
    private readonly ILogger<RepositoryUpdater> _logger;

    public RepositoryUpdater(
        IOptionsMonitor<LighthouseConfig> lighthouseConfig, 
        IFileUpdater fileUpdater,
        IGitService gitService,
        ILogger<RepositoryUpdater> logger)
    {
        _lighthouseConfig = lighthouseConfig.CurrentValue;
        _fileUpdater = fileUpdater;
        _gitService = gitService;
        _logger = logger;

        lighthouseConfig.OnChange(config => _lighthouseConfig = config);
    }

    public async Task<(bool, string?)> UpdateAsync(string repositoryName, Container container, string branchName = "main")
    {
        var repoConfig = GetRepositoryConfig(repositoryName);
        if (repoConfig == null)
        {
            _logger.LogError("Configuration for Repository: {repositoryName} not found. Update cancelled.", repositoryName);
            return (false, string.Empty);
        }

        _logger.LogInformation("Initiating update for repository: {repositoryName}.", repositoryName);

        using var repo = _gitService.CloneAndCheckout(repoConfig, branchName);
        if (repo == null)
        {
            _logger.LogWarning("Failed to clone repository: {repositoryName}. Update cancelled.", repositoryName);
            return (false, string.Empty);
        }

        if (!_gitService.CheckoutBranch(repo, branchName))
        {
            _logger.LogWarning("Failed to checkout branch: {branch} in repository: {repositoryName}. Update cancelled.", branchName, repositoryName);
            DeleteRepo(repo);
            return (false, string.Empty);
        }

        await ProcessFilesInRepository(repo, repoConfig, container);

        (bool didUpdateRepo, string? commitSha) = CommitAndPushChangesIfAny(repo, branchName, repoConfig, container);

        DeleteRepo(repo);
        return (didUpdateRepo, commitSha);
    }

    private RepositoryConfig? GetRepositoryConfig(string repositoryName)
    {
        return _lighthouseConfig.Repositories?.SingleOrDefault(r => r.Name == repositoryName);
    }

    private async Task ProcessFilesInRepository(Repository repo, RepositoryConfig repoConfig, Container container)
    {
        // Filter files by extension
        var validExtensions = new HashSet<string>(repoConfig.FileExtensions, StringComparer.OrdinalIgnoreCase);
        
        var files=
            Directory.GetFiles(
                repo.Info.WorkingDirectory, 
                "*", 
                SearchOption.AllDirectories
            )
            .Where(
                file => 
                    !file.Contains(LighthouseStrings.GitDirectory) && 
                    validExtensions.Contains(Path.GetExtension(file))
            )
            .ToList();

        _logger.LogInformation("Processing {fileCount} files in repository: {repositoryPath}.", files.Count, repo.Info.WorkingDirectory);

        foreach (var file in files)
        {
            var content = await File.ReadAllTextAsync(file);
            if (!content.Contains(LighthouseStrings.LighthouseTag))
                continue;

            var updatedContent = _fileUpdater.TryUpdate(content, container);
            if (content != updatedContent)
            {
                await File.WriteAllTextAsync(file, updatedContent);
                Commands.Stage(repo, file);
            }
        }
    }

    private (bool, string?) CommitAndPushChangesIfAny(Repository repo, string branchName, RepositoryConfig repoConfig, Container container)
    {
        if (!_gitService.HasChanges(repo)) return (false, string.Empty);

        _logger.LogInformation("Committing and pushing changes for repository: {repositoryName}.", repoConfig.Name);
        var commitSha = _gitService.CommitAndPushChanges(
            repo,
            branchName,
            repoConfig,
            $"Lighthouse update: {container.Host}/{container.Repository}:{container.Tag}"
        );
        return (true, commitSha);
    }

    private void DeleteRepo(Repository repo)
    {
        Directory.Delete(repo.Info.WorkingDirectory, true);
        _logger.LogDebug("Deleted local copy of repository from: {repositoryPath}.", repo.Info.WorkingDirectory);
    }
}