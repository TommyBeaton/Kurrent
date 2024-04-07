using Kurrent.Implementation;
using Kurrent.Interfaces;
using Kurrent.Models.Data;
using Kurrent.UnitTest.Helpers.Extensions;
using Kurrent.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Kurrent.UnitTest.Implementation;

public class NotificationHandlerTests
{
    private readonly Mock<INotifierFactory> _notifierFactoryMock = new();
    private readonly Mock<IOptionsMonitor<KurrentConfig>> _configMock = new();
    private readonly Mock<ILogger<NotificationHandler>> _loggerMock = new();
    private readonly Mock<INotifier> _notifierMock = new();

    private NotificationHandler GetSut(KurrentConfig config)
    {
        _configMock.Setup(x => x.CurrentValue).Returns(config);
        return new NotificationHandler(_notifierFactoryMock.Object, _configMock.Object,
            _loggerMock.Object);
    }
    
    
    [Fact]
    public async Task Send_Should_LogError_When_RepositoryConfigIsNotFound()
    {
        // Arrange
        var config = new KurrentConfig { Repositories = new List<RepositoryConfig>(0) };
        var sut = GetSut(config);

        var container = new Container { };
        var subscription = new SubscriptionConfig { RepositoryName = "UnknownRepo" };

        // Act
        await sut.Send(container, subscription, null);

        // Assert
        _loggerMock.VerifyLog(LogLevel.Error, $"Repository config not found for {subscription.RepositoryName}", Times.Once());
    }

    [Fact]
    public async Task Send_Should_LogError_When_NotifierConfigIsNotFound()
    {
        // Arrange
        var config = new KurrentConfig 
        {
            Repositories = new List<RepositoryConfig>
            {
                new RepositoryConfig { Name = "SomeRepo" }
            },
            Notifiers =new List<NotifierConfig>(0)
        };
        var sut = GetSut(config);

        var container = new Container { };
        var subscription = new SubscriptionConfig { RepositoryName = "SomeRepo", EventName = "UnknownEvent" };

        // Act
        await sut.Send(container, subscription, null);

        // Assert
        _loggerMock.VerifyLog(LogLevel.Error, $"Notifier config not found for {subscription.EventName}", Times.Once());
    }
    
    [Fact]
    public async Task Send_Should_LogError_When_NotifierIsNotFound()
    {
        // Arrange
        var notifiers = new List<NotifierConfig> { new NotifierConfig { Type = "knownType", EventName = "anyEvent"} };
        var repositories = new List<RepositoryConfig> { new RepositoryConfig { Name = "knownRepo" } };
        var config = new KurrentConfig { Notifiers = notifiers, Repositories = repositories };
        var sut = GetSut(config);

        var container = new Container { };
        var subscription = new SubscriptionConfig { RepositoryName = "knownRepo", EventName = "anyEvent" };

        _notifierFactoryMock.Setup(x => x.Create(It.IsAny<string>())).Returns((INotifier)null);

        // Act
        await sut.Send(container, subscription, null);

        // Assert
        _loggerMock.VerifyLog(LogLevel.Error, $"Notifier not found for {notifiers[0].Type}", Times.Once());
    }

    [Fact]
    public async Task Send_Should_CallNotifyAsync_When_AllValid()
    {
        // Arrange
        var notifiers = new List<NotifierConfig>
            { new NotifierConfig { Type = "knownType", EventName = "knownEvent" } };
        var repositories = new List<RepositoryConfig> { new RepositoryConfig { Name = "knownRepo" } };
        var config = new KurrentConfig { Notifiers = notifiers, Repositories = repositories };
        var sut = GetSut(config);

        var container = new Container { };
        var subscription = new SubscriptionConfig { RepositoryName = "knownRepo", EventName = "knownEvent" };
        _notifierFactoryMock.Setup(x => x.Create(It.IsAny<string>())).Returns(_notifierMock.Object);

        // Act
        await sut.Send(container, subscription, null);

        // Assert
        _notifierMock.Verify(x => x.NotifyAsync(container, repositories.First(), notifiers.First(), null), Times.Once);
    }
}