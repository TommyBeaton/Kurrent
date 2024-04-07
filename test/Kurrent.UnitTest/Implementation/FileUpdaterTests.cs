using FluentAssertions;
using FluentAssertions.Execution;
using Kurrent.Implementation;
using Kurrent.Models.Data;
using Microsoft.Extensions.Logging;
using Moq;

namespace Kurrent.UnitTest.Implementation;

public class FileUpdaterTests
{
    private readonly Mock<ILogger<FileUpdater>> _loggerMock;
    private readonly FileUpdater _sut;

    public FileUpdaterTests()
    {
        _loggerMock = new Mock<ILogger<FileUpdater>>();
        _sut = new FileUpdater(_loggerMock.Object);
    }

    [Fact]
    public void TryUpdate_WithMatchingTag_UpdatesLine()
    {
        // Arrange
        string host = "myregistry.azurecr.io";
        string repo = "hello-world";
        string tag(int version) => $"development-test-update{version}";
        string entry(int version) => $"acrImage: {host}/{repo}:{tag(version)} # kurrent update; regex: .*dev*.;";
        
        var content = $@"foo: hello there
{entry(8)}
noDomainImage: hello-world:development # kurrent update; regex: .*dev*.;
pollingImage: owdev.azurecr.io/epc-api/api:1.0.29802-development-613f742 # kurrent update; regex: .*dev*.;";
        var container = new Container
        {
            Tag = tag(9),
            Host = host,
            Repository = repo
        };

        // Act
        var result = _sut.TryUpdate(content, container);

        // Assert
        using var _ = new AssertionScope();
        result.Should().Contain(entry(9));
        result.Should().NotContain(entry(8));
    }
    
    [Fact]
    public void TryUpdate_WithMatchingTag_UpdatesMultipleLines()
    {
        // Arrange
        string host = "myregistry.azurecr.io";
        string repo = "hello-world";
        string tag(int version) => $"development-test-update{version}";
        string entry(int version) => $"acrImage: {host}/{repo}:{tag(version)} # kurrent update; regex: .*dev*.;";
        
        var content = $@"foo: hello there
{entry(8)}
{entry(8)}
noDomainImage: hello-world:development # kurrent update; regex: .*dev*.;
pollingImage: owdev.azurecr.io/epc-api/api:1.0.29802-development-613f742 # kurrent update; regex: .*dev*.;";
        var container = new Container
        {
            Tag = tag(9),
            Host = host,
            Repository = repo
        };

        // Act
        var result = _sut.TryUpdate(content, container);

        // Assert
        using var _ = new AssertionScope();
        var lines = result.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        lines.Count(line => line.Contains(entry(9))).Should().Be(2); // check updated entries
        lines.Count(line => line.Contains(entry(8))).Should().Be(0); // check original entries
    }

    [Fact]
    public void TryUpdate_WithoutMatchingTag_DoesNotAlterLine()
    {
        // Arrange
        var content = @"foo: hello there
acrImage: myregistry.azurecr.io/hello-world:development-test-update8 # lighthouse update; regex: .*dev*.;
noDomainImage: hello-world:development # lighthouse update; regex: .*dev*.;
pollingImage: owdev.azurecr.io/epc-api/api:1.0.29802-development-613f742 # lighthouse update; regex: .*dev*.;";
        var container = new Container
        {
            Tag = "production-update1",
            Host = "myregistry.azurecr.io",
            Repository = "hello-world"
        };

        // Act
        var result = _sut.TryUpdate(content, container);

        // Assert
        result.Should().Be(content);
    }
}