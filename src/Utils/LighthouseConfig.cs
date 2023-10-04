namespace Lighthouse.Utils;

public class LighthouseConfig
{
    public List<WebhookConfig>? Webhooks { get; set; }
    public List<SubscriptionConfig>? Subscriptions { get; set; }
    public List<RepositoryConfig>? Repositories { get; set; }
}

public class WebhookConfig
{
    public string Name { get; set; }
    public string Path { get; set; }
    public string Type { get; set; }
}

public class SubscriptionConfig
{
    public string Name { get; set; }
    public string WebhookName { get; set; }
    public string RepositoryName { get; set; }
    public string Branch { get; set; }
}

public class RepositoryConfig
{
    public string Name { get; set; }
    public string Url { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }  // Consider using a more secure method for storing passwords.
}
