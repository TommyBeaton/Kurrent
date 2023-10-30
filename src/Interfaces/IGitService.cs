using Kurrent.Utils;
using LibGit2Sharp;

namespace Kurrent.Interfaces;

public interface IGitService
{
    public Repository? CloneAndCheckout(
        RepositoryConfig repositoryConfig,
        string branchName);

    public bool CheckoutBranch(
        Repository repo,
        string branchName);
    
    public string CommitAndPushChanges(
        Repository repo, 
        string branchName, 
        RepositoryConfig repositoryConfig, 
        string commitMessage = "");

    public bool HasChanges(Repository repo);
} 