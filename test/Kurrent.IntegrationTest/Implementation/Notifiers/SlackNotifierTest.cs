using System.Net;
using FluentAssertions.Execution;
using Kurrent.Implementation.Notifications.Notifiers;
using Kurrent.IntegrationTest.Utils;
using Kurrent.Models.Data;
using Kurrent.Utils;
using Microsoft.Extensions.Logging.Abstractions;

namespace Kurrent.IntegrationTest.Implementation.Notifiers;

public class SlackNotifierTest
{
    SlackNotifier _slackNotifier;
    
    public SlackNotifierTest()
    {
        IHttpClientFactory clientFactory = new KurrentHttpClientFactory();
        _slackNotifier = new SlackNotifier(clientFactory, new NullLogger<SlackNotifier>());
    }
    
    [Fact]
    public async Task Should_Send_Message()
    {
        var kurrentConfig = TestUtils.GetConfiguration();
        var slackToken = kurrentConfig.Notifiers[0].Token;
        var channel = "U03G66AH3NG";
        Image image = new Image(Repository: "integration", Tag: "test");
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
        var result = await _slackNotifier.NotifyAsync(image, repositoryConfig, notifierConfig, "some-commit-sha");
        using var _ = new AssertionScope();
        Assert.True(result.IsSuccess);
        Assert.Null(result.Error);
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    }
}