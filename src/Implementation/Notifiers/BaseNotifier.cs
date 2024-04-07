using Kurrent.Interfaces;
using Kurrent.Models.Data;
using Kurrent.Models.Data.Notifiers;
using Kurrent.Utils;

namespace Kurrent.Implementation.Notifiers;

public abstract class BaseNotifier : INotifier
{
    public abstract Task<NotifierResult> NotifyAsync(
        Container container, 
        RepositoryConfig repositoryConfig,
        NotifierConfig notifierConfig,
        string? commitSha);

    protected string GetPlainMessage(Container container, RepositoryConfig repositoryConfig, string? commitSha)
    {
        string text =
            $"Kurrent update: New image {container.ImageName} was updated in {repositoryConfig.Name} {KurrentStrings.GetEmoji()}";
        
        text += string.IsNullOrEmpty(commitSha) ? 
            "" : $"\n Commit sha: {commitSha}";
        
        if (repositoryConfig.Url.Contains("github.com"))
            text += string.IsNullOrEmpty(commitSha) ? 
                "" : $"\n [View the changes here]({repositoryConfig.Url}/commit/{commitSha})";

        return text;
    }
}