using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using Lighthouse.Interfaces;
using Lighthouse.Utils;

namespace Lighthouse.Implementation;

public class GitService : IGitService
{
    private readonly ILogger<GitService> _logger;

    public GitService(ILogger<GitService> logger)
    {
        _logger = logger;
    }
    
    public Repository? CloneAndCheckout(
        RepositoryConfig repositoryConfig,
        string branchName)
    {
        Repository repo;
        
        var cloneOptions = new CloneOptions
        {
            CredentialsProvider = GetCredentialsHandler(repositoryConfig)
        };

        try
        {
            var localPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var repoPath = Repository.Clone(repositoryConfig.Url, localPath, cloneOptions);
            repo = new Repository(repoPath);
            Commands.Checkout(repo, repo.Branches[branchName]);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error cloning repository");
            return null;
        }
        
        return repo;
    }

    public bool CheckoutBranch(Repository repo, string branchName)
    {
        Branch branch;
        
        try
        {
            branch = repo.Branches[branchName];
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error finding branch: {branchName}", branchName);
            return false;
        }
        
        if (branch == null)
        {
            _logger.LogError("Branch: {branchName} not found.", branchName);
            return false;
        }
        
        Commands.Checkout(repo, branch);
        return true;
    }

    public void CommitAndPushChanges(
        Repository repo, 
        string branchName, 
        RepositoryConfig repositoryConfig,
        string commitMessage = "")
    {
        _logger.LogTrace("Committing changes to repository: {repositoryName}", repositoryConfig.Name);
        
        var signature = new Signature("Lighthouse Service", "service@lighthouse.com", DateTimeOffset.Now);
        repo.Commit(commitMessage, signature, signature);
        
        var pushOptions = new PushOptions
        {
            CredentialsProvider = GetCredentialsHandler(repositoryConfig)
        };
        
        _logger.LogInformation("Pushing changes to repository: {repository}", repositoryConfig.Name);
        repo.Network.Push(repo.Branches[branchName], pushOptions);
    }

    public bool HasChanges(Repository repo)
    {
        return repo.RetrieveStatus().IsDirty;
    }
    
    private  CredentialsHandler GetCredentialsHandler(RepositoryConfig repositoryConfig)
    {
        return (_, _, _) => new UsernamePasswordCredentials
        {
            Username = repositoryConfig.Username,
            Password = repositoryConfig.Password
        };
    }
}