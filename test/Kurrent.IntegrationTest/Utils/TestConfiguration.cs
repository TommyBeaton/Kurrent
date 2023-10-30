namespace Kurrent.IntegrationTest.Utils;

public class TestConfiguration
{
    public Credentials Credentials { get; set; }
}

public class Credentials
{
    public string SlackToken { get; set; }
}