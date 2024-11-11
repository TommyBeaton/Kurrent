using Kurrent.Implementation;
using Kurrent.Interfaces;
using Kurrent.Interfaces.Git;
using Kurrent.Interfaces.Notifications;
using Kurrent.Models.Data;
using Kurrent.UnitTest.Helpers.Extensions;
using Kurrent.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Kurrent.UnitTest.Implementation;

public class SubscriptionHandlerTests
{
    private readonly Mock<IRepositoryUpdater> _repositoryUpdaterMock;
    private readonly Mock<INotificationHandler> _notificationHandlerMock;
    private readonly Mock<IOptionsMonitor<AppConfig>> _kurrentConfigMock;
    private readonly Mock<ILogger<SubscriptionHandler>> _loggerMock;

    private const string RepoName = "Test Repo";
    private const string EventName = "Test Event";
    private const string BranchName = "Test Branch";
    private readonly Image _image = new Image
    {
        Host = "testHost",
        Repository = "testRepo",
        Tag = "testTag"
    };
    
    private readonly AppConfig _config = new()
    {
        Repositories = new List<RepositoryConfig>
        {
            new()
            {
                Name = RepoName,
                EventSubscriptions = new List<string>{EventName},
                Branch = BranchName
            }
        }
    };

    public SubscriptionHandlerTests()
    {
        _repositoryUpdaterMock = new();
        _notificationHandlerMock = new();
        _kurrentConfigMock = new();
        _loggerMock = new();
    }

    private SubscriptionHandler CreateSut(RepositoryConfig? config = null)
    {
        config ??= _config.Repositories[0];

        var appConfig = new AppConfig
        {
            Repositories = new List<RepositoryConfig> { config }
        };
        
        _kurrentConfigMock.Setup(x => x.CurrentValue).Returns(appConfig);
        
        return new SubscriptionHandler(
            _repositoryUpdaterMock.Object,
            _kurrentConfigMock.Object,
            _notificationHandlerMock.Object,
            _loggerMock.Object
        );
    }
    
    [Fact]
    public async Task UpdateAsync_ReturnsOnNoSubscribers()
    {
        // Arrange
        string eventName = EventName + " not found";
        string expectedLog = $"No repo config found for event {EventName}. Cancelling update.";
        
        var sut = CreateSut(new RepositoryConfig());
        
        // Act
        await sut.UpdateAsync(EventName, new Image());

        // Assert
        _loggerMock.VerifyLog(LogLevel.Warning, expectedLog, Times.Once());
        _repositoryUpdaterMock.VerifyNoOtherCalls();
        _notificationHandlerMock.VerifyNoOtherCalls();
    }
    
    [Fact]
    public async Task UpdateAsync_UpdatesSubscribers()
    {
        // Arrange 
        var sut = CreateSut();
        _repositoryUpdaterMock.Setup(m => m.UpdateAsync(_config.Repositories.First(), _image))
            .ReturnsAsync((true, null));
        
        // Act
        await sut.UpdateAsync(EventName, _image);

        // Assert
        _repositoryUpdaterMock.Verify(m => m.UpdateAsync(_config.Repositories.First(), _image), Times.Once);
        _notificationHandlerMock.Verify(m => m.Send(_image, _config.Repositories.First(), EventName, null), Times.Once);
    }
    
    [Fact]
    public async Task UpdateAsync_DoesntSendNotification_When_DidntUpdateRepo()
    {
        // Arrange 
        var sut = CreateSut();
        _repositoryUpdaterMock.Setup(m => m.UpdateAsync(_config.Repositories.First(), _image))
            .ReturnsAsync((false, null));
        
        // Act
        await sut.UpdateAsync(EventName, _image);

        // Assert
        _repositoryUpdaterMock.Verify(m => m.UpdateAsync(_config.Repositories.First(), _image), Times.Once);
        _notificationHandlerMock.VerifyNoOtherCalls();
    }
    
    [Fact]
    public async Task UpdateAsync_SendsCommitSha_WhenNotNull()
    {
        // Arrange
        string commitSha = Guid.NewGuid().ToString();
        var sut = CreateSut();
        _repositoryUpdaterMock.Setup(m => m.UpdateAsync(_config.Repositories.First(), _image))
            .ReturnsAsync((true, commitSha));
        
        // Act
        await sut.UpdateAsync(EventName, _image);

        // Assert
        _repositoryUpdaterMock.Verify(m => m.UpdateAsync(_config.Repositories.First(), _image), Times.Once);
        _notificationHandlerMock.Verify(m => m.Send(_image, _config.Repositories.First(), EventName, commitSha), Times.Once);
    }
}