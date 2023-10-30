using System.Text.Json.Serialization;

namespace Kurrent.Models.Data.Webhooks;

public class AcrEvent
{
    [JsonPropertyName("id")]
    public string Id { get; set; }
    
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }
    
    [JsonPropertyName("action")]
    public string Action { get; set; }
    
    [JsonPropertyName("target")]
    public AcrTarget? Target { get; set; }
    
    [JsonPropertyName("request")]
    public AcrRequest? Request { get; set; }
}

public class AcrTarget
{
    [JsonPropertyName("digest")]
    public string Digest { get; set; }
    
    [JsonPropertyName("mediaType")]
    public string MediaType { get; set; }
    
    [JsonPropertyName("size")]
    public int Size { get; set; }
    
    [JsonPropertyName("repository")]
    public string Repository { get; set; }
    
    [JsonPropertyName("tag")]
    public string Tag { get; set; }
    
    [JsonPropertyName("length")]
    public int Length { get; set; }
    
    string GetImageName()
    {
        return $"{Repository}:{Tag}";
    }
}

public class AcrRequest
{
    [JsonPropertyName("id")]
    public string Id { get; set; }
    
    [JsonPropertyName("host")]
    public string Host { get; set; }
    
    [JsonPropertyName("method")]
    public string Method { get; set; }
    
    [JsonPropertyName("useragent")]
    public string UserAgent { get; set; }
}