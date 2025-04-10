using IgorVonNyssen.NINA.DlLink.DlLinkDrivers;
using Moq;
using Moq.Protected;
using NINA.Equipment.Interfaces;
using System.Net;

namespace IgorVonNyssen.NINA.DlLink.Tests {

    public class DlLinkDriverTests {

        [Fact]
        public async Task Connect_ShouldReturnTrue_WhenResponseIsSuccessful() {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage {
                    StatusCode = HttpStatusCode.MultiStatus,
                    Content = new StringContent("[\"Outlet1\", \"Outlet2\"]")
                })
                .Verifiable();

            var httpClient = new HttpClient(handlerMock.Object);

            var dlLinkDriver = new DlLinkDriver("TestDevice", httpClient, "localhost");

            // Act
            var result = await dlLinkDriver.Connect(CancellationToken.None);

            // Assert
            Assert.True(result);
            Assert.True(dlLinkDriver.Connected);
            Assert.Equal(2, ((ISwitchHub)dlLinkDriver).Switches.Count);
        }

        [Fact]
        public async Task Connect_ShouldReturnFalse_WhenResponseIsUnsuccessful() {
            // Arrange
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

            var dlLinkDriver = new DlLinkDriver("TestDevice", httpClient, "localhost");

            // Act
            var result = await dlLinkDriver.Connect(CancellationToken.None);

            // Assert
            Assert.False(result);
            Assert.False(dlLinkDriver.Connected);
            Assert.Empty(((ISwitchHub)dlLinkDriver).Switches);
        }

        [Fact]
        public async Task Connect_ShouldReturnWithoutOutlets_WhenResponseIsEmpty() {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage {
                    StatusCode = HttpStatusCode.MultiStatus,
                    Content = new StringContent("[]")
                })
                .Verifiable();

            var httpClient = new HttpClient(handlerMock.Object);

            var dlLinkDriver = new DlLinkDriver("TestDevice", httpClient, "localhost");

            // Act
            var result = await dlLinkDriver.Connect(CancellationToken.None);

            // Assert
            Assert.True(result);
            Assert.True(dlLinkDriver.Connected);
            Assert.Empty(((ISwitchHub)dlLinkDriver).Switches);
        }

        [Fact]
        public async Task Connect_ShouldReturnFalse_WhenResponseIsInvalidJson() {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage {
                    StatusCode = HttpStatusCode.MultiStatus,
                    Content = new StringContent("")
                })
                .Verifiable();

            var httpClient = new HttpClient(handlerMock.Object);

            var dlLinkDriver = new DlLinkDriver("TestDevice", httpClient, "localhost");

            // Act
            var result = await dlLinkDriver.Connect(CancellationToken.None);

            // Assert
            Assert.False(result);
            Assert.False(dlLinkDriver.Connected);
            Assert.Empty(((ISwitchHub)dlLinkDriver).Switches);
        }

        [Fact]
        public async Task Connect_ShouldReturnFalse_WhenServerTimesOut() {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ThrowsAsync(new TaskCanceledException())
                .Verifiable();

            var httpClient = new HttpClient(handlerMock.Object);

            var dlLinkDriver = new DlLinkDriver("TestDevice", httpClient, "localhost");

            // Act
            var result = await dlLinkDriver.Connect(CancellationToken.None);

            // Assert
            Assert.False(result);
            Assert.False(dlLinkDriver.Connected);
            Assert.Empty(((ISwitchHub)dlLinkDriver).Switches);
        }
    }
}