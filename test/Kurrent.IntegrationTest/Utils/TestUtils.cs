using Kurrent.Utils;
using Microsoft.Extensions.Configuration;

namespace Kurrent.IntegrationTest.Utils;

public static class TestUtils
{
    public static AppConfig GetConfiguration()
    {
        //load configuration from appsettings.json
        IConfiguration configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.Local.json")
            .AddEnvironmentVariables()
            .Build();
        
        //map configuration to TestConfiguration
        return configuration.GetSection("Kurrent").Get<AppConfig>();
    }
}