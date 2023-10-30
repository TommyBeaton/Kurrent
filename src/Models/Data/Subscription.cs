namespace Kurrent.Models.Data;

public record Subscription(string RepositoryName, string WebhookName, string Name, string Branch = "main");