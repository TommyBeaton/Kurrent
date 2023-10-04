using System.Text.RegularExpressions;
using LibGit2Sharp;
using Lighthouse.Interfaces;
using Lighthouse.Models.Data;
using Lighthouse.Utils;
using Microsoft.Extensions.Options;

namespace Lighthouse.Extensions;

public class RepositoryUpdater : IRepositoryUpdater
{
    private readonly LighthouseConfig _lighthouseConfig;
    private readonly ILogger<RepositoryUpdater> _logger;

    public RepositoryUpdater(
        IOptions<LighthouseConfig> lighthouseConfig, 
        ILogger<RepositoryUpdater> logger)
    {
        _lighthouseConfig = lighthouseConfig.Value;
        _logger = logger;
    }


    public async Task UpdateAsync(string repositoryName, Container container, string branchName = "main")
    {
        var repoConfig = _lighthouseConfig.Repositories?.Single(r => r.Name == repositoryName);

        if (repoConfig == null)
        {
            _logger.LogError("Repository: {repositoryName} not found. Cancelling update.", repositoryName);
            return;
        }
        
        _logger.LogError("Starting update for repository: {repositoryName}", repositoryName);
        
        
        var cloneOptions = new CloneOptions
        {
            CredentialsProvider = (_url, _user, _cred) => new UsernamePasswordCredentials
            {
                Username = repoConfig.Username,
                Password = repoConfig.Password
            }
        };
        var localPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var repoPath = Repository.Clone(repoConfig.Url, localPath, cloneOptions);
        
        using var repo = new Repository(repoPath);
        var branch = repo.Branches[branchName];
        
        if (branch == null)
        {
            _logger.LogError("Branch: {branchName} not found. Cancelling update.", branchName);
            return;
        }
        
        Commands.Checkout(repo, branch);

        // Search and update files
        foreach (var file in Directory.GetFiles(repoPath, "*", SearchOption.AllDirectories))
        {
            var content = File.ReadAllText(file);

            if (!content.Contains("# lighthouse update;"))
                continue;

            var updatedContent = UpdateImageTagInContent(content, container);

            if (content != updatedContent)
            {
                File.WriteAllText(file, updatedContent);
                Commands.Stage(repo, file);
            }
        }
        
        var status = repo.RetrieveStatus();
        bool hasChanges = status.IsDirty;
        
        // Commit changes if there are any staged changes
        if (hasChanges)
        {
            var signature = new Signature("Lighthouse Service", "lighthouse@example.com", DateTimeOffset.Now);
            repo.Commit($"Lighthouse update: {container.Host}/{container.Repository}:{container.Tag}", signature, signature);
            
        }
    }
    
    private string UpdateImageTagInContent(string content, Container container)
    {
        var regex = new Regex($"({Regex.Escape(container.Host)}/)?{Regex.Escape(container.Repository)}:[^\\s]+ # lighthouse update; regex: (.*);");
        return regex.Replace(content, match =>
        {
            var pattern = match.Groups[2].Value;

            if (Regex.IsMatch(container.Tag, pattern))
            {
                return $"{container.Host}/{container.Repository}:{container.Tag} # lighthouse update; regex: {pattern};";
            }

            return match.Value;
        });
    }
}