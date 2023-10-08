namespace Lighthouse.Models.Data;

public record Container(string Host = "", string Repository = "", string Tag = "")
{
    public string ImageName => $"{Repository}:{Tag}";

    public override string ToString()
    {
        return !string.IsNullOrEmpty(Host) ? $"{Host}/{ImageName}" : ImageName;
    }

    public bool IsValid => !string.IsNullOrEmpty(Repository) && !string.IsNullOrEmpty(Tag);
    
    public bool HasHost => !string.IsNullOrEmpty(Host);
}