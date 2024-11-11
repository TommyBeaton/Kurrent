using Kurrent.Utils;
using LibGit2Sharp;

namespace Kurrent.Interfaces.Git;

public interface IGitService
{
    public Repository? CloneAndCheckout(
        RepositoryConfig repositoryConfig);

    public bool CheckoutBranch(
        Repository repo,
        string branchName);
    
    public string CommitAndPushChanges(
        Repository clonedRepo,
        RepositoryConfig repositoryConfig,
        string commitMessage = "");

    public bool HasChanges(Repository repo);
} 