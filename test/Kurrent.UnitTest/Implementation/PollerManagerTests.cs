using FluentAssertions;
using Kurrent.Implementation;
using Kurrent.Implementation.Polling;
using Kurrent.Interfaces;
using Kurrent.Interfaces.Polling;
using Kurrent.UnitTest.Helpers.Extensions;
using Kurrent.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Kurrent.UnitTest.Implementation;

public class PollerManagerTests
{
    private readonly Mock<IOptionsMonitor<AppConfig>> _configMock;
    private readonly Mock<IPollerFactory> _pollerFactoryMock;
    private readonly Mock<ILogger<PollerManager>> _loggerMock;
    private readonly Mock<IPoller> _pollerMock;

    public PollerManagerTests()
    {
        _configMock = new Mock<IOptionsMonitor<AppConfig>>();
        _pollerFactoryMock = new Mock<IPollerFactory>();
        _loggerMock = new Mock<ILogger<PollerManager>>();
        _pollerMock = new Mock<IPoller>();
    }

    private PollerManager GetSut(AppConfig config)
    {
        _configMock.Setup(x => x.CurrentValue).Returns(config);
        return new PollerManager(_configMock.Object, _pollerFactoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task StartAsync_Should_StartPollersWhenPollersExist()
    {
        // Arrange
        var pollers = new List<PollerConfig> { new PollerConfig { Type = "knownType" } };
        var config = new AppConfig { Pollers = pollers };

        _pollerFactoryMock.Setup(x => x.Create(It.IsAny<string>())).Returns(_pollerMock.Object);

        var sut = GetSut(config);
        
        // Act
        await sut.StartAsync();

        // Assert
        _pollerMock.Verify(x => x.Start(It.IsAny<PollerConfig>(), It.IsAny<CancellationToken>()), Times.Once);
        _loggerMock.VerifyLog(LogLevel.Information, "Starting pollers", Times.Once());
    }

    [Fact]
    public async Task StartAsync_Should_LogWhenNoPollersExist()
    {
        // Arrange
        var config = new AppConfig { Pollers = new List<PollerConfig>() };
        var sut = GetSut(config);

        // Act
        await sut.StartAsync();

        // Assert
        _loggerMock.VerifyLog(LogLevel.Warning, "No pollers found. This could be expected behaviour if none are configured.", Times.Once());
    }

    [Fact]
    public async Task StartAsync_Should_ThrowException_When_UnknownPollerType()
    {
        // Arrange
        var pollers = new List<PollerConfig> { new PollerConfig { Type = "UnknownType" } };
        var config = new AppConfig { Pollers = pollers };

        _pollerFactoryMock.Setup(x => x.Create(It.IsAny<string>())).Returns((IPoller)null);

        var sut = GetSut(config);
        
        // Act 
        var exception = await Record.ExceptionAsync(async () => await sut.StartAsync());

        // Assert
        exception.Should().BeOfType<ArgumentOutOfRangeException>().Which.Message.Should()
            .Be($"Poller not found for {pollers.First().Type} (Parameter 'Type')");
    }

    [Fact]
    public async Task StopAsync_Should_StopPollersAndLogError_When_ExceptionOccurs()
    {
        // Arrange
        _pollerMock.Setup(x => x.Start(It.IsAny<PollerConfig>(), It.IsAny<CancellationToken>()))
            .Throws(new Exception("Test exception"));

        var pollers = new List<PollerConfig> { new PollerConfig { Type = "knownType" } };
        var config = new AppConfig { Pollers = pollers };
        
        _pollerFactoryMock.Setup(x => x.Create(It.IsAny<string>())).Returns(_pollerMock.Object);
        var sut = GetSut(config);

        await sut.StartAsync();

        // Act
        await sut.StopAsync();

        // Assert
        _loggerMock.VerifyLog(LogLevel.Error, "Exception while stopping pollers", Times.Once());
    }

    [Fact]
    public async Task StartAsync_Should_StartPollersGracefully_When_NoExceptionOccurs()
    {
        // Arrange
        var fakePoller = new FakePoller();
        var pollers = new List<PollerConfig> { new PollerConfig { Type = "knownType" } };
        var config = new AppConfig { Pollers = pollers };

        _pollerFactoryMock.Setup(x => x.Create(It.IsAny<string>())).Returns(fakePoller);
        var sut = GetSut(config);

        // Act
        await sut.StartAsync();

        // Assert
        fakePoller.AutoResetStartEvent.WaitOne(5000); //Block the tests thread to allow Start to run in the poller's thread. 
        fakePoller.Started.Should().BeTrue();
        _loggerMock.VerifyLog(LogLevel.Information, "Starting pollers", Times.Once());
    }

    [Fact]
    public async Task StopAsync_Should_StopPollersGracefully_When_NoExceptionOccurs()
    {
        // Arrange
        var fakePoller = new FakePoller();
        var pollers = new List<PollerConfig> { new PollerConfig { Type = "knownType" } };
        var config = new AppConfig { Pollers = pollers };

        _pollerFactoryMock.Setup(x => x.Create(It.IsAny<string>())).Returns(fakePoller);
        var sut = GetSut(config);

        await sut.StartAsync();

        // Act
        await sut.StopAsync();

        // Assert
        fakePoller.AutoResetStopEvent.WaitOne(5000); //Block the tests thread to allow Stop to run in the poller's thread. 
        fakePoller.Stopped.Should().BeTrue();
        _loggerMock.VerifyLog(LogLevel.Information, "Stopping pollers", Times.Once());
    }
}

public class FakePoller : IPoller
{
    public bool Started { get; private set; } = false;
    public bool Stopped { get; private set; } = false;
    public AutoResetEvent AutoResetStartEvent { get; private set; } = new AutoResetEvent(false);
    public AutoResetEvent AutoResetStopEvent { get; private set; } = new AutoResetEvent(false);

    private CancellationToken _ct;
    
    Timer _timer;
    
    public void Start(PollerConfig config, CancellationToken token)
    {
        _timer = new Timer(Stop, null, 0, 1);
        
        Started = true;
        AutoResetStartEvent.Set();
        _ct = token;
    }

    private void Stop(object? state)
    {
        if(_ct.IsCancellationRequested) //Check if we should stop
            Stop();
    }
    
    public void Stop()
    {
        Stopped = true;
        AutoResetStopEvent.Set();
    }
}