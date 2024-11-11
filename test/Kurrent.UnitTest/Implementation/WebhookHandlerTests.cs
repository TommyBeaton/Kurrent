using FluentAssertions;
using FluentAssertions.Execution;
using Kurrent.Implementation;
using Kurrent.Interfaces;
using Kurrent.Models.Data;
using Kurrent.UnitTest.Helpers.Extensions;
using Kurrent.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;

namespace Kurrent.UnitTest.Implementation;

public class WebhookHandlerTests
{
    private readonly Mock<ISubscriptionHandler> _subscriptionHandlerMock;
    private readonly Mock<ILogger<WebhookHandler>> _loggerMock;

    private readonly WebhookHandler _sut;

    public WebhookHandlerTests()
    {
        _subscriptionHandlerMock = new Mock<ISubscriptionHandler>();
        _loggerMock = new Mock<ILogger<WebhookHandler>>();
        _sut = new WebhookHandler(
            _subscriptionHandlerMock.Object, 
            _loggerMock.Object);
    }

    [Fact]
    public async Task ProcessRequestAsync_ReturnsValidImage_FromDocker()
    {
        // Arrange
        var jsonFilePath = "Helpers/Data/dockerhub_valid.json";
        var requestBody = File.ReadAllText(jsonFilePath);
        string webhookType = "docker";

        var context = BuildContext(requestBody);
        var config = BuildConfig(webhookType);

        // Act
        await _sut.ProcessRequestAsync(context, config);

        // Assert
        _subscriptionHandlerMock.Verify(x => x.UpdateAsync(
            config.EventName,
            It.Is<Image>(x =>
                x.Repository == "test-repo" &&
                x.Tag == "test-tag" &&
                x.IsValid)), Times.Once);
    }
    
    [Fact]
    public async Task ProcessRequestAsync_ReturnsValidImage_FromAcr()
    {
        // Arrange
        var jsonFilePath = "Helpers/Data/acr_valid.json";
        var requestBody = File.ReadAllText(jsonFilePath);
        string webhookType = "acr";
        
        var context = BuildContext(requestBody);
        var config = BuildConfig(webhookType);

        // Act
        await _sut.ProcessRequestAsync(context, config);

        // Assert
        _subscriptionHandlerMock.Verify(x => x.UpdateAsync(
            config.EventName,
            It.Is<Image>(x =>
                x.Repository == "hello-world" &&
                x.Tag == "some-acr-tag" &&
                x.Host == "myregistry.azurecr.io" &&
                x.IsValid)), Times.Once);
    }

    [Fact]
    public async Task ProcessRequestAsync_UnsupportedWebhookType_LogsErrorAndReturnsInvalidContainer()
    {
        // Arrange
        var requestBody = "some body";
        var webhookType = "unsupportedType";
    
        var context = BuildContext(requestBody);
        var config = BuildConfig(webhookType);

        // Act
        await _sut.ProcessRequestAsync(context, config);

        // Assert
        _loggerMock.VerifyLog(LogLevel.Error, $"Webhook type {webhookType} not supported", Times.Once());
        _subscriptionHandlerMock.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("docker")]
    [InlineData("acr")]
    public async Task ProcessRequestAsync_FailingParsing_LogsErrorAndReturnsInvalidContainer(string webhookType)
    {
        // Arrange
        var requestBody = "foo";

        var context = BuildContext(requestBody);
        var config = BuildConfig(webhookType);

        // Act
        await _sut.ProcessRequestAsync(context, config);

        // Assert
        _loggerMock.VerifyLog(LogLevel.Error, "'foo' is an invalid JSON literal", Times.Once());
        _subscriptionHandlerMock.VerifyNoOtherCalls();
    }

    private HttpContext BuildContext(string body)
    {
        var context = new DefaultHttpContext();
        
        var memoryStream = new MemoryStream();
        var writer = new StreamWriter(memoryStream);
        writer.Write(body);
        writer.Flush();
        memoryStream.Position = 0; // Reset stream position to the beginning

        context.Request.Body = memoryStream;
        context.Request.ContentLength = memoryStream.Length;
        
        return context;
    }

    private WebhookConfig BuildConfig(string webhookType, string eventName = "event")
    {
        return new WebhookConfig
        {
            Type = webhookType,
            EventName = eventName,
        };
    }
}