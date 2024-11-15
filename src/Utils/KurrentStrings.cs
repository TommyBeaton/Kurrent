namespace Kurrent.Utils;

public static class KurrentStrings
{
    public const string Acr = "acr";
    public const string Docker = "docker";
    public const string Slack = "slack";
    public const string GitDirectory = ".git";
    public const string KurrentTag = "# kurrent update;";
    public const string K8s = "k8s";
    public static string[] Emojis = new[] { "🔥", "🌊", "😎", "🐳", "💯", "🔧", "🪄", "🎊", "🎉", "🛥️", "🚀", "🛸", "🛰️", "🌟", "🥵" };
    public static string GetRandomEmoji() => Emojis[new Random().Next(0, Emojis.Length)];
    public const string InformationBlue = "#006ee6";
    public const string MessageHeader = "Kurrent updated an image 🌊";
    public static string MessageBody(string image, string repositoryUrl, string repositoryName) => $"Image: {image} was updated in <{repositoryUrl}|{repositoryName}> {GetRandomEmoji()}";
    public static string MessageFooter(string commitSha) => $"Commit: {commitSha}";
    public static string CommitLink(string commitSha, string repositoryUrl) => $"<{repositoryUrl}/commit/{commitSha}|View changes in GitHub>";
}