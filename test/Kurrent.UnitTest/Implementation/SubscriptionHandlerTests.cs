using Kurrent.Implementation;
using Kurrent.Interfaces;
using Kurrent.Models.Data;
using Kurrent.UnitTest.Helpers.Extensions;
using Kurrent.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Kurrent.UnitTest.Implementation;

public class SubscriptionHandlerTests
{
    private readonly Mock<IRequestHandler> _requestHandlerMock;
    private readonly Mock<IRepositoryUpdater> _repositoryUpdaterMock;
    private readonly Mock<INotificationHandler> _notificationHandlerMock;
    private readonly Mock<IOptionsMonitor<KurrentConfig>> _kurrentConfigMock;
    private readonly Mock<ILogger<SubscriptionHandler>> _loggerMock;

    private const string RepoName = "Test Repo";
    private const string EventName = "Test Event";
    private const string BranchName = "Test Branch";
    private readonly Container _container = new Container
    {
        Host = "testHost",
        Repository = "testRepo",
        Tag = "testTag"
    };
    
    private readonly KurrentConfig _config = new()
    {
        Subscriptions = new List<SubscriptionConfig>
        {
            new()
            {
                RepositoryName = RepoName,
                EventName = EventName,
                Branch = BranchName
            }
        }
    };

    public SubscriptionHandlerTests()
    {
        _requestHandlerMock = new();
        _repositoryUpdaterMock = new();
        _notificationHandlerMock = new();
        _kurrentConfigMock = new();
        _loggerMock = new();
    }
    
    [Fact]
    public async Task UpdateFromWebhook_ValidRequest_UpdatesSubscribers()
    {
        // Arrange
        var sut = GetSut();
        
        // Act
        await sut.UpdateFromWebhookAsync(EventName, "some type", "some body");

        // Assert
        _repositoryUpdaterMock.Verify(m => m.UpdateAsync(It.IsAny<string>(), _container, It.IsAny<string>()), Times.AtLeastOnce);
        _notificationHandlerMock.Verify(m => m.Send(It.IsAny<Container>(), It.IsAny<SubscriptionConfig>(), It.IsAny<string>()), Times.AtLeastOnce);
    }
    
    [Fact]
    public async Task UpdateFromWebhook_ReturnsOnInvalidContainer()
    {
        // Arrange
        string expectedLog = $"Container image not found in request body for webhook {EventName}. Cancelling update";
        
        var mockContainer = new Container();
        
        

        var sut = GetSut(container: mockContainer);

        // Act
        await sut.UpdateFromWebhookAsync(EventName, "some type", "some body");

        // Assert
        _loggerMock.VerifyLog(LogLevel.Information, expectedLog, Times.Once());
        _repositoryUpdaterMock.Verify(m => m.UpdateAsync(It.IsAny<string>(), mockContainer, It.IsAny<string>()), Times.Never);
        _notificationHandlerMock.Verify(m => m.Send(It.IsAny<Container>(), It.IsAny<SubscriptionConfig>(), It.IsAny<string>()), Times.Never);
    }
    
    [Fact]
    public async Task UpdateFromWebhook_ReturnsOnNoSubscribers()
    {
        // Arrange
        string eventName = EventName + " not found";
        string expectedLog = $"No subscriber found for event {EventName}. Cancelling update.";

        var config = new KurrentConfig
        {
            Subscriptions = new List<SubscriptionConfig>
            {
                new()
                {
                    RepositoryName = RepoName,
                    EventName = eventName,
                    Branch = BranchName
                }
            }
        };

        var sut = GetSut(config);

        // Act
        await sut.UpdateFromWebhookAsync(EventName, "some type", "some body");

        // Assert
        _loggerMock.VerifyLog(LogLevel.Warning, expectedLog, Times.Once());
        _repositoryUpdaterMock.Verify(m => m.UpdateAsync(It.IsAny<string>(), _container, It.IsAny<string>()), Times.Never);
        _notificationHandlerMock.Verify(m => m.Send(It.IsAny<Container>(), It.IsAny<SubscriptionConfig>(), It.IsAny<string>()), Times.Never);
    }
    
    [Fact]
    public async Task UpdateFromPoller_ReturnsOnNoSubscribers()
    {
        // Arrange
        string eventName = EventName + " not found";
        string expectedLog = $"No subscriber found for event {EventName}. Cancelling update.";

        var config = new KurrentConfig
        {
            Subscriptions = new List<SubscriptionConfig>
            {
                new()
                {
                    RepositoryName = RepoName,
                    EventName = eventName,
                    Branch = BranchName
                }
            }
        };

        var sut = GetSut(config);

        // Act
        await sut.UpdateFromPollerAsync(EventName, _container);

        // Assert
        _loggerMock.VerifyLog(LogLevel.Warning, expectedLog, Times.Once());
        _repositoryUpdaterMock.Verify(m => m.UpdateAsync(It.IsAny<string>(), _container, It.IsAny<string>()), Times.Never);
        _notificationHandlerMock.Verify(m => m.Send(It.IsAny<Container>(), It.IsAny<SubscriptionConfig>(), It.IsAny<string>()), Times.Never);
    }
    
    [Fact]
    public async Task UpdateFromPoller_ValidRequest_UpdatesSubscribers()
    {
        // Arrange
        var sut = GetSut();
        
        // Act
        await sut.UpdateFromPollerAsync(EventName, _container);

        // Assert
        _repositoryUpdaterMock.Verify(m => m.UpdateAsync(It.IsAny<string>(), _container, It.IsAny<string>()), Times.AtLeastOnce);
        _notificationHandlerMock.Verify(m => m.Send(It.IsAny<Container>(), It.IsAny<SubscriptionConfig>(), It.IsAny<string>()), Times.AtLeastOnce);
    }

    private SubscriptionHandler GetSut(KurrentConfig? kurrentConfig = null, Container? container = null, bool didUpdate = true, string commitSha = "some-sha")
    {
        var config = kurrentConfig ?? _config;
        _kurrentConfigMock.Setup(x => x.CurrentValue).Returns(config);

        var mockContainer = container ?? _container;
        _requestHandlerMock.Setup(m => m.GetTagFromRequest(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(mockContainer);
        _repositoryUpdaterMock.Setup(x => x.UpdateAsync(It.IsAny<string>(), It.IsAny<Container>(), It.IsAny<string>()))
            .ReturnsAsync((didUpdate, commitSha));
        
        return new SubscriptionHandler(
            _requestHandlerMock.Object,
            _repositoryUpdaterMock.Object,
            _kurrentConfigMock.Object,
            _notificationHandlerMock.Object,
            _loggerMock.Object
        );
    }
}