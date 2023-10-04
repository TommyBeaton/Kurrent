using System.Text.Json.Serialization;

namespace Lighthouse.Models.Data.Webhooks;

public class DockerHubEvent
{
    [JsonPropertyName("callback_url")]
    public string CallbackUrl { get; set; } 
    
}

public class DockerHubPushData
{
    [JsonPropertyName("pushed_at")]
    public int PushedAt { get; set; }
    
    [JsonPropertyName("pusher")]
    public string Pusher { get; set; }
    
    [JsonPropertyName("tag")]
    public string Tag { get; set; }
}

public class DockerHubRepository
{
    [JsonPropertyName("comment_count")]
    public int CommentCount { get; set; }
    
    [JsonPropertyName("date_created")]
    public int DateCreated { get; set; }
    
    [JsonPropertyName("description")]
    public string Description { get; set; }
    
    [JsonPropertyName("dockerfile")]
    public string Dockerfile { get; set; }
    
    [JsonPropertyName("full_description")]
    public int FullDescription { get; set; }
    
    [JsonPropertyName("is_official")]
    public bool IsOfficial { get; set; }
    
    [JsonPropertyName("is_private")]
    public bool IsPrivate { get; set; }
    
    [JsonPropertyName("is_trusted")]
    public bool IsTrusted { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; }
    
    [JsonPropertyName("namespace")]
    public string Namespace { get; set; }
    
    [JsonPropertyName("owner")]
    public string Owner { get; set; }
    
    [JsonPropertyName("repo_name")]
    public int RepoName { get; set; }
    
    [JsonPropertyName("repo_url")]
    public int RepoUrl { get; set; }
    
    [JsonPropertyName("star_count")]
    public int StarCount { get; set; }
    
    [JsonPropertyName("status")]
    public int Status { get; set; }
}