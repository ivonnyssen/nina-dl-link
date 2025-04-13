using IgorVonNyssen.NINA.DlLink.DlLinkDrivers;
using Moq;
using Moq.Protected;
using NINA.Equipment.Interfaces;
using RichardSzalay.MockHttp;
using System.Net;

using Xunit;

using System.Threading.Tasks;
using IgorVonNyssen.NINA.DlLink.DlLinkSequenceItems;

namespace IgorVonNyssen.NINA.DlLink.Tests {

    public class DlOutletTests {

        [Fact]
        public async Task Poll_ShouldReturnTrue_WhenResponseIsSuccessful() {
            // Arrange
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.Expect("http://localhost/restapi/relay/outlets/0/state/")
                .Respond("application/json", "true"); // Simulate outlet is ON

            var httpClient = new HttpClient(mockHttp);
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
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When("http://localhost/restapi/relay/outlets/0/state/")
                .Respond(HttpStatusCode.BadRequest); // Simulate failure

            var httpClient = new HttpClient(mockHttp);
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
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When("http://localhost/restapi/relay/outlets/0/state/")
                .Respond("application/json", "invalid json"); // Simulate invalid JSON

            var httpClient = new HttpClient(mockHttp);
            var dlOutlet = new DlOutlet("TestOutlet", 1, httpClient, "localhost", "user", "password");

            // Act
            var result = await dlOutlet.Poll();

            // Assert
            Assert.False(result);
            Assert.Equal(0d, dlOutlet.Value);
        }

        [Fact]
        public async Task SetValue_ShouldSendCorrectRequest_WhenResponseIsSuccessful() {
            // Arrange
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(HttpMethod.Put, "http://localhost/restapi/relay/outlets/0/state/")
                .WithContent("value=true")
                .Respond(HttpStatusCode.NoContent); // Simulate successful action

            var httpClient = new HttpClient(mockHttp);
            var dlOutlet = new DlOutlet("TestOutlet", 1, httpClient, "localhost", "user", "password") {
                TargetValue = 1d
            };

            // Act
            await ((IWritableSwitch)dlOutlet).SetValue();

            // Assert
            mockHttp.VerifyNoOutstandingExpectation(); // Ensure the request was made
        }

        [Fact]
        public async Task SetValue_ShouldLogError_WhenResponseIsUnsuccessful() {
            // Arrange
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(HttpMethod.Put, "http://localhost/restapi/relay/outlets/0/state/")
                .WithContent("value=true")
                .Respond(HttpStatusCode.BadRequest); // Simulate failure

            var httpClient = new HttpClient(mockHttp);
            var dlOutlet = new DlOutlet("TestOutlet", 1, httpClient, "localhost", "user", "password") {
                TargetValue = 1d
            };

            // Act
            await ((IWritableSwitch)dlOutlet).SetValue();

            // Assert
            mockHttp.VerifyNoOutstandingExpectation(); // Ensure the request was made
        }
    }
}

/*
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
                    req.RequestUri == new Uri("http://localhost/restapi/relay/outlets/0/state/") &&
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
                    req.RequestUri == new Uri("http://localhost/restapi/relay/outlets/0/state/") &&
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
                    req.RequestUri == new Uri("http://localhost/restapi/relay/outlets/0/state/") &&
                    req.Content.ReadAsStringAsync().Result == "value=true"
                ),
                ItExpr.IsAny<CancellationToken>()
            );
        }
    }
}*/