using IgorVonNyssen.NINA.DlLink.DlLinkDrivers;
using Moq;
using Moq.Protected;
using NINA.Equipment.Interfaces;
using System.Net;

namespace IgorVonNyssen.NINA.DlLink.Tests {

    public class DlOutletTests {

        [Fact]
        public async Task Poll_ShouldReturnTrue_WhenResponseIsSuccessful() {
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
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("true")
                })
                .Verifiable();

            var httpClient = new HttpClient(handlerMock.Object);

            var dlOutlet = new DlOutlet("TestOutlet", 1, httpClient, "localhost", "user", "password");

            // Act
            var result = await dlOutlet.Poll();

            // Assert
            Assert.True(result);
            Assert.Equal(1d, dlOutlet.Value);
        }

        [Fact]
        public async Task Poll_ShouldReturnFalse_WhenResponseIsUnsuccessful() {
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

            var dlOutlet = new DlOutlet("TestOutlet", 1, httpClient, "localhost", "user", "password");

            // Act
            var result = await dlOutlet.Poll();

            // Assert
            Assert.False(result);
            Assert.Equal(0d, dlOutlet.Value);
        }

        [Fact]
        public async Task Poll_ShouldReturnFalse_WhenHttpRequestExceptionOccurs() {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ThrowsAsync(new HttpRequestException("Request failed"))
                .Verifiable();

            var httpClient = new HttpClient(handlerMock.Object);

            var dlOutlet = new DlOutlet("TestOutlet", 1, httpClient, "localhost", "user", "password");

            // Act
            var result = await dlOutlet.Poll();

            // Assert
            Assert.False(result);
            Assert.Equal(0d, dlOutlet.Value);
        }

        [Fact]
        public async Task Poll_ShouldReturnFalse_WhenJsonExceptionOccurs() {
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
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("invalid json")
                })
                .Verifiable();

            var httpClient = new HttpClient(handlerMock.Object);

            var dlOutlet = new DlOutlet("TestOutlet", 1, httpClient, "localhost", "user", "password");

            // Act
            var result = await dlOutlet.Poll();

            // Assert
            Assert.False(result);
            Assert.Equal(0d, dlOutlet.Value);
        }

        [Fact]
        public async Task SetValue_ShouldSetCorrectValue_WhenResponseIsSuccessful() {
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
                    StatusCode = HttpStatusCode.NoContent
                })
                .Verifiable();

            var httpClient = new HttpClient(handlerMock.Object);

            var dlOutlet = new DlOutlet("TestOutlet", 1, httpClient, "localhost", "user", "password") {
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

            var dlOutlet = new DlOutlet("TestOutlet", 1, httpClient, "localhost", "user", "password") {
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
        public async Task SetValue_ShouldLogError_WhenHttpRequestExceptionOccurs() {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ThrowsAsync(new HttpRequestException("Request failed"))
                .Verifiable();

            var httpClient = new HttpClient(handlerMock.Object);

            var dlOutlet = new DlOutlet("TestOutlet", 1, httpClient, "localhost", "user", "password") {
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
}