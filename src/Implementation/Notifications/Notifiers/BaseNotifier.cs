using Kurrent.Interfaces.Notifications;
using Kurrent.Models.Data;
using Kurrent.Models.Data.Notifiers;
using Kurrent.Utils;

namespace Kurrent.Implementation.Notifications.Notifiers;

public abstract class BaseNotifier : INotifier
{
    public abstract Task<NotifierResult> NotifyAsync(
        Image image, 
        RepositoryConfig repositoryConfig,
        NotifierConfig notifierConfig,
        string? commitSha);

    protected string GetPlainMessage(Image image, RepositoryConfig repositoryConfig, string? commitSha)
    {
        string text =
            $"Kurrent update: New image {image.Name} was updated in {repositoryConfig.Name} {KurrentStrings.GetRandomEmoji()}";
        
        text += string.IsNullOrEmpty(commitSha) ? 
            "" : $"\n Commit sha: {commitSha}";
        
        if (repositoryConfig.Url.Contains("github.com"))
            text += string.IsNullOrEmpty(commitSha) ? 
                "" : $"\n [View the changes here]({repositoryConfig.Url}/commit/{commitSha})";

        return text;
    }
}