using Lighthouse.Utils;
using Microsoft.Extensions.Configuration;

namespace Lighthouse.IntegrationTest.Utils;

public static class TestUtils
{
    public static LighthouseConfig GetConfiguration()
    {
        //load configuration from appsettings.json
        IConfiguration configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.Local.json")
            .AddEnvironmentVariables()
            .Build();
        
        //map configuration to TestConfiguration
        return configuration.GetSection("Lighthouse").Get<LighthouseConfig>();
    }
}