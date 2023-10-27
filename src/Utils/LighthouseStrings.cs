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
}