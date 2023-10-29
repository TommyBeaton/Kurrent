using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions.Execution;
using Lighthouse.Implementation.Notifiers;
using Lighthouse.IntegrationTest.Utils;
using Lighthouse.Models.Data;
using Lighthouse.Utils;
using Microsoft.Extensions.Logging.Abstractions;

namespace Lighthouse.IntegrationTest.Implementation.Notifiers;

public class SlackNotifierTest
{
    SlackNotifier _slackNotifier;
    
    public SlackNotifierTest()
    {
        IHttpClientFactory clientFactory = new LighthouseHttpClientFactory();
        _slackNotifier = new SlackNotifier(clientFactory, new NullLogger<SlackNotifier>());
    }
    
    [Fact]
    public async Task Should_Send_Message()
    {
        var lighthouseConfig = TestUtils.GetConfiguration();
        var slackToken = lighthouseConfig.Notifiers[0].Token;
        var channel = "U03G66AH3NG";
        Container container = new Container(Repository: "integration", Tag: "test");
        RepositoryConfig repositoryConfig = new RepositoryConfig
        {
            Url = "github.com/test/test",
            Name = "test"
        };
        NotifierConfig notifierConfig = new NotifierConfig
        {
            Channel = channel,
            Token = slackToken,
            Type = "slack",
            Name = "slack-test"
        };
        var result = await _slackNotifier.NotifyAsync(container, repositoryConfig, notifierConfig, "some-commit-sha");
        using var _ = new AssertionScope();
        Assert.True(result.IsSuccess);
        Assert.Null(result.Error);
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    }
}