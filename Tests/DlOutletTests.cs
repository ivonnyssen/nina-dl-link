using Xunit;
using Moq;
using NINA.Profile.Interfaces;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using IgorVonNyssen.NINA.DlLink;
using IgorVonNyssen.NINA.DlLink.DlLinkDrivers;
using Moq.Protected;
using NINA.Equipment.Interfaces;

public class DlOutletTests {

    [Fact]
    public async Task Poll_ShouldReturnTrue_WhenResponseIsSuccessful() {
        // Arrange
        var mockPluginSettings = new Mock<IPluginOptionsAccessor>();
        mockPluginSettings.Setup(p => p.GetValueString(nameof(DlLink.DLUserName), string.Empty)).Returns("user");
        mockPluginSettings.Setup(p => p.GetValueString(nameof(DlLink.DLPassword), string.Empty)).Returns("password");
        mockPluginSettings.Setup(p => p.GetValueString(nameof(DlLink.DLServerAddress), string.Empty)).Returns("localhost");

        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("true")
            })
            .Verifiable();

        var httpClient = new HttpClient(handlerMock.Object);

        var dlOutlet = new DlOutlet("TestOutlet", 1, mockPluginSettings.Object, httpClient);

        // Act
        var result = await dlOutlet.Poll();

        // Assert
        Assert.True(result);
        Assert.Equal(1d, dlOutlet.Value);
    }

    [Fact]
    public async Task Poll_ShouldReturnFalse_WhenResponseIsUnsuccessful() {
        // Arrange
        var mockPluginSettings = new Mock<IPluginOptionsAccessor>();
        mockPluginSettings.Setup(p => p.GetValueString(nameof(DlLink.DLUserName), string.Empty)).Returns("user");
        mockPluginSettings.Setup(p => p.GetValueString(nameof(DlLink.DLPassword), string.Empty)).Returns("password");
        mockPluginSettings.Setup(p => p.GetValueString(nameof(DlLink.DLServerAddress), string.Empty)).Returns("localhost");

        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent("Bad Request")
            })
            .Verifiable();

        var httpClient = new HttpClient(handlerMock.Object);

        var dlOutlet = new DlOutlet("TestOutlet", 1, mockPluginSettings.Object, httpClient);

        // Act
        var result = await dlOutlet.Poll();

        // Assert
        Assert.False(result);
        Assert.Equal(0d, dlOutlet.Value);
    }

    [Fact]
    public async Task SetValue_ShouldSetCorrectValue_WhenResponseIsSuccessful() {
        // Arrange
        var mockPluginSettings = new Mock<IPluginOptionsAccessor>();
        mockPluginSettings.Setup(p => p.GetValueString(nameof(DlLink.DLUserName), string.Empty)).Returns("user");
        mockPluginSettings.Setup(p => p.GetValueString(nameof(DlLink.DLPassword), string.Empty)).Returns("password");
        mockPluginSettings.Setup(p => p.GetValueString(nameof(DlLink.DLServerAddress), string.Empty)).Returns("localhost");

        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage {
                StatusCode = HttpStatusCode.NoContent
            })
            .Verifiable();

        var httpClient = new HttpClient(handlerMock.Object);

        var dlOutlet = new DlOutlet("TestOutlet", 1, mockPluginSettings.Object, httpClient) {
            TargetValue = 1d
        };

        // Act
        await ((IWritableSwitch)dlOutlet).SetValue();

        // Assert
        handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Put &&
                req.RequestUri == new Uri("http://localhost/restapi/relay/outlets/1/state/") &&
                req.Content.ReadAsStringAsync().Result == "value=true"
            ),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    [Fact]
    public async Task SetValue_ShouldLogError_WhenResponseIsUnsuccessful() {
        // Arrange
        var mockPluginSettings = new Mock<IPluginOptionsAccessor>();
        mockPluginSettings.Setup(p => p.GetValueString(nameof(DlLink.DLUserName), string.Empty)).Returns("user");
        mockPluginSettings.Setup(p => p.GetValueString(nameof(DlLink.DLPassword), string.Empty)).Returns("password");
        mockPluginSettings.Setup(p => p.GetValueString(nameof(DlLink.DLServerAddress), string.Empty)).Returns("localhost");

        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent("Bad Request")
            })
            .Verifiable();

        var httpClient = new HttpClient(handlerMock.Object);

        var dlOutlet = new DlOutlet("TestOutlet", 1, mockPluginSettings.Object, httpClient) {
            TargetValue = 1d
        };

        // Act
        await ((IWritableSwitch)dlOutlet).SetValue();

        // Assert
        handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Put &&
                req.RequestUri == new Uri("http://localhost/restapi/relay/outlets/1/state/") &&
                req.Content.ReadAsStringAsync().Result == "value=true"
            ),
            ItExpr.IsAny<CancellationToken>()
        );
    }
}