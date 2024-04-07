using FluentAssertions;
using FluentAssertions.Execution;
using Kurrent.Implementation;
using Kurrent.UnitTest.Helpers.Extensions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Kurrent.UnitTest.Implementation;

public class RequestHandlerTests
{
    private readonly Mock<ILogger<RequestHandler>> _loggerMock;

    private readonly RequestHandler _sut;

    public RequestHandlerTests()
    {
        _loggerMock = new Mock<ILogger<RequestHandler>>();
        _sut = new RequestHandler(_loggerMock.Object);
    }

    [Fact]
    public void GetTagFromDockerRequest_ValidRequest_ReturnsValidContainer()
    {
        // Arrange
        var jsonFilePath = "Helpers/Data/dockerhub_valid.json";
        var requestBody = File.ReadAllText(jsonFilePath);
        string webhookType = "docker";

        // Act
        var result = _sut.GetTagFromRequest(requestBody, webhookType);

        // Assert
        using var _ = new AssertionScope();
        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
        result.Repository.Should().Be("test-repo");
        result.Tag.Should().Be("test-tag");
    }
    
    [Fact]
    public void GetTagFromAcrRequest_ValidRequest_ReturnsValidContainer()
    {
        // Arrange
        var jsonFilePath = "Helpers/Data/acr_valid.json";
        var requestBody = File.ReadAllText(jsonFilePath);
        string webhookType = "acr";

        // Act
        var result = _sut.GetTagFromRequest(requestBody, webhookType);

        // Assert
        using var _ = new AssertionScope();
        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
        result.Host.Should().Be("myregistry.azurecr.io");
        result.Repository.Should().Be("hello-world");
        result.Tag.Should().Be("some-acr-tag");
    }

    [Fact]
    public void GetTagFromRequest_UnsupportedWebhookType_LogsErrorAndReturnsInvalidContainer()
    {
        // Arrange
        var requestBody = "some body";
        var webhookType = "unsupportedType";
    
        // Act
        var result = _sut.GetTagFromRequest(requestBody, webhookType);

        // Assert
        using var _ = new AssertionScope();
        result.IsValid.Should().BeFalse();
        _loggerMock.VerifyLog(LogLevel.Error, $"Webhook type {webhookType} not supported", Times.Once());
    
    }

    [Theory]
    [InlineData("docker")]
    [InlineData("acr")]
    public void GetTagFromRequest_FailingParsing_LogsErrorAndReturnsInvalidContainer(string webhookType)
    {
        // Arrange
        var requestBody = "foo";

        // Act
        var result = _sut.GetTagFromRequest(requestBody, webhookType);

        // Assert
        using var _ = new AssertionScope();
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        _loggerMock.VerifyLog(LogLevel.Error, "'foo' is an invalid JSON literal", Times.Once());
    }
    
    [Theory]
    [InlineData("docker")]
    [InlineData("acr")]
    public void GetTagFromDockerRequest_FailingParsing_LogsErrorAndReturnsInvalidContainer(string webhookType)
    {
        // Arrange
        var jsonFilePath = "helpers/data/invalid.json";
        var requestBody = File.ReadAllText(jsonFilePath);

        // Act
        var result = _sut.GetTagFromRequest(requestBody, webhookType);

        // Assert
        using var _ = new AssertionScope();
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        _loggerMock.VerifyLog(LogLevel.Error, $"Could not deserialize {webhookType} request.", Times.Once());
        
    }

}