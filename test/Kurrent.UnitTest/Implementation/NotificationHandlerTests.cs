using Kurrent.Implementation;
using Kurrent.Implementation.Notifications;
using Kurrent.Interfaces;
using Kurrent.Interfaces.Notifications;
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
    private readonly Mock<IOptionsMonitor<AppConfig>> _configMock = new();
    private readonly Mock<ILogger<NotificationHandler>> _loggerMock = new();
    private readonly Mock<INotifier> _notifierMock = new();

    private readonly string _eventName = "Event";
    
    private NotificationHandler GetSut(AppConfig config)
    {
        _configMock.Setup(x => x.CurrentValue).Returns(config);
        return new NotificationHandler(
            _notifierFactoryMock.Object,
            _configMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Send_Should_LogError_When_NotifierConfigIsNotFound()
    {
        // Arrange
        var config = new AppConfig 
        {
            Notifiers =new List<NotifierConfig>()
        };
        var sut = GetSut(config);

        var image = new Image();
        var repoConfig = new RepositoryConfig() { Name = "SomeRepo", EventSubscriptions = new List<string>{$"{_eventName}FooBar"} };

        // Act
        await sut.Send(image, repoConfig, _eventName, null);

        // Assert
        _loggerMock.VerifyLog(LogLevel.Warning, $"No Notifier configs found for event: {_eventName}", Times.Once());
    }
    
    [Fact]
    public async Task Send_Should_LogError_When_NotifierTypeIsNotFound()
    {
        // Arrange
        var notifiers = new List<NotifierConfig>
        {
            new NotifierConfig
            {
                Type = "knownType", 
                EventSubscriptions = new List<string>{_eventName}
            }
        };
        var repositories = new List<RepositoryConfig> 
            { new RepositoryConfig { Name = "knownRepo" } };
        var config = new AppConfig { Notifiers = notifiers, Repositories = repositories };
        var sut = GetSut(config);

        var image = new Image();
        var repoConfig = new RepositoryConfig() { Name = "SomeRepo", EventSubscriptions = new List<string>{_eventName} };

        _notifierFactoryMock.Setup(x => x.Create(It.IsAny<string>())).Returns((INotifier)null);

        // Act
        await sut.Send(image, repoConfig, _eventName, null);

        // Assert
        _loggerMock.VerifyLog(LogLevel.Error, $"Notifier not found for {notifiers[0].Type}", Times.Once());
    }

    [Fact]
    public async Task Send_Should_CallNotifyAsync_When_AllValid()
    {
        // Arrange
        var expectedNotifier = "testNotifier";
        var expectedRepo = "testRepo";
        var notifiers = new List<NotifierConfig> { new NotifierConfig { Name = expectedNotifier, Type = "knownType", EventSubscriptions = new List<string>{_eventName} } };
        var config = new AppConfig { Notifiers = notifiers };
        var sut = GetSut(config);

        var image = new Image();
        var repoConfig = new RepositoryConfig() { Name = expectedRepo, EventSubscriptions = new List<string>{_eventName} };
        _notifierFactoryMock.Setup(x => x.Create(It.IsAny<string>())).Returns(_notifierMock.Object);

        // Act
        await sut.Send(image, repoConfig, _eventName, null);

        // Assert
        _notifierMock.Verify(x => x.NotifyAsync(image, repoConfig, It.Is<NotifierConfig>(x => x.Name == expectedNotifier), null), Times.Once);
    }
}