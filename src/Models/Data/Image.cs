namespace Kurrent.Models.Data;

public record Image(string Host = "", string Repository = "", string Tag = "")
{
    public string Name => $"{Repository}:{Tag}";

    public override string ToString()
    {
        return !string.IsNullOrEmpty(Host) ? $"{Host}/{Name}" : Name;
    }

    public bool IsValid => !string.IsNullOrEmpty(Repository) && !string.IsNullOrEmpty(Tag);
    
    public bool HasHost => !string.IsNullOrEmpty(Host);
}