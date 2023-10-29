using Lighthouse.Models.Data;

namespace Lighthouse.Utils;

public static class LighthouseStrings
{
    public const string Acr = "acr";
    public const string Docker = "docker";
    public const string Teams = "docker";
    public const string Slack = "docker";
    public const string GitDirectory = ".git";
    public const string LighthouseTag = "# lighthouse update;";
    public const string K8s = "k8s";
    public static string[] Emojis = new[] { "🔥", "🌊", "😎", "🐳", "💯", "🔧", "🪄", "🎊", "🎉", "🛥️", "🚀", "🛸", "🛰️", "🌟", "🥵" };
    public static string GetEmoji() => Emojis[new Random().Next(0, Emojis.Length)];
    public const string InformationBlue = "#006ee6";
    public const string MessageHeader = "Kurrent updated an image 🌊";
    public static string MessageBody(string image, string repositoryUrl, string repositoryName) => $"Image: {image} was updated in <https://{repositoryUrl}|{repositoryName}> {GetEmoji()}";
    public static string MessageFooter(string commitSha) => $"Commit: {commitSha}";
    public static string CommitLink(string commitSha, string repositoryUrl) => $"<https://{repositoryUrl}/commit/{commitSha}|View changes in GitHub>";
}