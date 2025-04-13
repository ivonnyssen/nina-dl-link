using IgorVonNyssen.NINA.DlLink.DlLinkDrivers;
using IgorVonNyssen.NINA.DlLink.DlLinkSequenceItems;
using RichardSzalay.MockHttp;
using System.Net;

namespace IgorVonNyssen.NINA.DlLink.Tests {

    public class HttpUtilsTests {

        [Fact]
        public async Task GetOutletNames_ShouldReturnOutletNames_WhenResponseIsSuccessful() {
            // Arrange
            var serverAddress = "localhost";
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.Expect($"http://{serverAddress}/restapi/relay/outlets/all;/name/")
                    .Respond(HttpStatusCode.MultiStatus, "application/json", "[\"Outlet1\", \"Outlet2\"]");

            var mockHttpClient = new HttpClient(mockHttp);

            // Act
            var result = await HttpUtils.GetOutletNames(mockHttpClient, serverAddress, CancellationToken.None);

            // Assert
            Assert.True(result.IsOk);
            Assert.Equal(["Outlet1", "Outlet2"], result.Value);
            mockHttp.VerifyNoOutstandingExpectation(); // Ensure the request was made
            mockHttp.VerifyNoOutstandingRequest();
        }

        [Fact]
        public async Task GetOutletNames_ShouldReturnError_WhenResponseIsUnsuccessful() {
            // Arrange
            var serverAddress = "localhost";
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.Expect($"http://{serverAddress}/restapi/relay/outlets/all;/name/")
                    .Respond(HttpStatusCode.BadRequest);

            var mockHttpClient = new HttpClient(mockHttp);

            // Act
            var result = await HttpUtils.GetOutletNames(mockHttpClient, serverAddress, CancellationToken.None);

            // Assert
            Assert.True(result.IsErr);
            mockHttp.VerifyNoOutstandingExpectation(); // Ensure the request was made
            mockHttp.VerifyNoOutstandingRequest();
        }

        [Fact]
        public async Task GetOutletState_ShouldReturnTrue_WhenOutletIsOn() {
            // Arrange
            var serverAddress = "localhost";
            var outletNumber = 1;
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.Expect($"http://{serverAddress}/restapi/relay/outlets/{outletNumber - 1}/state/")
                    .Respond("application/json", "true");

            var mockHttpClient = new HttpClient(mockHttp);

            // Act
            var result = await HttpUtils.GetOutletState(mockHttpClient, serverAddress, outletNumber, CancellationToken.None);

            // Assert
            Assert.True(result.IsOk);
            Assert.True(result.Value);
            mockHttp.VerifyNoOutstandingExpectation(); // Ensure the request was made
            mockHttp.VerifyNoOutstandingRequest();
        }

        [Fact]
        public async Task GetOutletState_ShouldReturnError_WhenOutletNumberIsInvalid() {
            // Arrange
            var serverAddress = "localhost";
            var outletNumber = 0; // Invalid outlet number
            var mockHttp = new MockHttpMessageHandler();

            var mockHttpClient = new HttpClient(mockHttp);

            // Act
            var result = await HttpUtils.GetOutletState(httpClient: mockHttpClient, serverAddress, outletNumber, CancellationToken.None);

            // Assert
            Assert.True(result.IsErr);
            mockHttp.VerifyNoOutstandingExpectation(); // Ensure there was no request made
            mockHttp.VerifyNoOutstandingRequest();
        }

        [Fact]
        public async Task SetOutletState_ShouldReturnSuccess_WhenStateIsSetSuccessfully() {
            // Arrange
            var serverAddress = "localhost";
            var outletNumber = 1;
            var valueToSet = true;
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.Expect($"http://{serverAddress}/restapi/relay/outlets/{outletNumber - 1}/state/")
                    .Respond(HttpStatusCode.NoContent);

            var mockHttpClient = new HttpClient(mockHttp);

            // Act
            var result = await HttpUtils.SetOutletState(mockHttpClient, serverAddress, outletNumber, valueToSet, CancellationToken.None);

            // Assert
            Assert.True(result.IsOk);
            mockHttp.VerifyNoOutstandingExpectation(); // Ensure the request was made
            mockHttp.VerifyNoOutstandingRequest();
        }

        [Fact]
        public async Task TriggerOutletAction_ShouldReturnSuccess_WhenActionIsCycle() {
            // Arrange
            var serverAddress = "localhost";
            var outletNumber = 1;
            var action = OutletActions.Cycle;
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.Expect($"http://{serverAddress}/restapi/relay/outlets/{outletNumber - 1}/cycle/")
                    .Respond(HttpStatusCode.NoContent);

            var mockHttpClient = new HttpClient(mockHttp);

            // Act
            var result = await HttpUtils.TriggerOutletAction(mockHttpClient, serverAddress, outletNumber, action, CancellationToken.None);

            // Assert
            Assert.True(result.IsOk);
            mockHttp.VerifyNoOutstandingExpectation(); // Ensure the request was made
            mockHttp.VerifyNoOutstandingRequest();
        }

        [Fact]
        public async Task TriggerOutletAction_ShouldReturnError_WhenActionIsInvalid() {
            // Arrange
            var serverAddress = "localhost";
            var outletNumber = 1;
            var action = (OutletActions)999; // Invalid action
            var mockHttp = new MockHttpMessageHandler();

            var mockHttpClient = new HttpClient(mockHttp);

            // Act
            var result = await HttpUtils.TriggerOutletAction(mockHttpClient, serverAddress, outletNumber, action, CancellationToken.None);

            // Assert
            Assert.True(result.IsErr);
            mockHttp.VerifyNoOutstandingExpectation(); // Ensure no request was made
            mockHttp.VerifyNoOutstandingRequest();
        }

        [Fact]
        public async Task SetOutletState_ShouldReturnError_WhenOutletNumberIsInvalid() {
            // Arrange
            var serverAddress = "localhost";
            var outletNumber = 0; // Invalid outlet number
            var valueToSet = true;
            var mockHttp = new MockHttpMessageHandler();

            var mockHttpClient = new HttpClient(mockHttp);

            // Act
            var result = await HttpUtils.SetOutletState(mockHttpClient, serverAddress, outletNumber, valueToSet, CancellationToken.None);

            // Assert
            Assert.True(result.IsErr);
            mockHttp.VerifyNoOutstandingExpectation(); // Ensure there was no request made
            mockHttp.VerifyNoOutstandingRequest();
        }

        [Fact]
        public async Task TriggerOutletAction_ShouldReturnError_WhenOutletNumberIsInvalid() {
            // Arrange
            var serverAddress = "localhost";
            var outletNumber = 0; // Invalid outlet number
            var action = OutletActions.On;
            var mockHttp = new MockHttpMessageHandler();

            var mockHttpClient = new HttpClient(mockHttp);

            // Act
            var result = await HttpUtils.TriggerOutletAction(mockHttpClient, serverAddress, outletNumber, action, CancellationToken.None);

            // Assert
            Assert.True(result.IsErr);
            mockHttp.VerifyNoOutstandingExpectation(); // Ensure there was no request made
            mockHttp.VerifyNoOutstandingRequest();
        }

        [Theory]
        [InlineData(OutletActions.On, "value=true")]
        [InlineData(OutletActions.Off, "value=false")]
        public async Task TriggerOutletAction_ShouldSendCorrectRequest_WhenResponseIsSuccessful(OutletActions action, string expectedPayload) {
            // Arrange
            var serverAddress = "localhost";
            var outletNumber = 1;
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.Expect(HttpMethod.Put, $"http://{serverAddress}/restapi/relay/outlets/{outletNumber - 1}/state/")
                .WithFormData(expectedPayload)
                .Respond(HttpStatusCode.NoContent); // Simulate successful action

            var httpClient = new HttpClient(mockHttp);

            // Act
            var result = await HttpUtils.TriggerOutletAction(httpClient, "localhost", outletNumber, action, default);

            // Assert
            Assert.True(result.IsOk);
            mockHttp.VerifyNoOutstandingExpectation(); // Ensure the request was made
            mockHttp.VerifyNoOutstandingRequest();
        }

        [Theory]
        [InlineData(OutletActions.On, "value=true")]
        [InlineData(OutletActions.Off, "value=false")]
        public async Task TriggerOutletAction_ShouldReturnError_WhenResponseIsUnsuccessful(OutletActions action, string expectedPayload) {
            // Arrange
            var serverAddress = "localhost";
            var outletNumber = 1;
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.Expect(HttpMethod.Put, $"http://{serverAddress}/restapi/relay/outlets/{outletNumber - 1}/state/")
                .WithFormData(expectedPayload)
                .Respond(HttpStatusCode.BadRequest); // Simulate failure

            var httpClient = new HttpClient(mockHttp);

            // Act
            var result = await HttpUtils.TriggerOutletAction(httpClient, "localhost", outletNumber, action, default);

            // Assert
            Assert.False(result.IsOk);
            mockHttp.VerifyNoOutstandingExpectation(); // Ensure the request was made
            mockHttp.VerifyNoOutstandingRequest();
        }

        [Fact]
        public async Task TriggerOutletAction_ShouldReturnErrorWhenCyclingInvalidOutletNumber() {
            // Arrange
            var serverAddress = "localhost";
            var outletNumber = 0; // Invalid outlet number
            var action = OutletActions.Cycle;
            var mockHttp = new MockHttpMessageHandler();
            var mockHttpClient = new HttpClient(mockHttp);
            // Act
            var result = await HttpUtils.TriggerOutletAction(mockHttpClient, serverAddress, outletNumber, action, CancellationToken.None);
            // Assert
            Assert.True(result.IsErr);
            mockHttp.VerifyNoOutstandingExpectation(); // Ensure there was no request made
            mockHttp.VerifyNoOutstandingRequest();
        }

        [Fact]
        public async Task TriggerOutletAction_ShouldReturnErrorWhenCyclingResponseIsUnsuccessful() {
            // Arrange
            var serverAddress = "localhost";
            var outletNumber = 1;
            var action = OutletActions.Cycle;
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.Expect($"http://{serverAddress}/restapi/relay/outlets/{outletNumber - 1}/cycle/")
                    .Respond(HttpStatusCode.BadRequest); // Simulate failure
            var mockHttpClient = new HttpClient(mockHttp);
            // Act
            var result = await HttpUtils.TriggerOutletAction(mockHttpClient, serverAddress, outletNumber, action, CancellationToken.None);
            // Assert
            Assert.True(result.IsErr);
            mockHttp.VerifyNoOutstandingExpectation(); // Ensure the request was made
            mockHttp.VerifyNoOutstandingRequest();
        }

        [Fact]
        public async Task TriggerOutletActuion_ShouldReturnErrorWhenRequestIsCancelled() {
            // Arrange
            var serverAddress = "localhost";
            var outletNumber = 1;
            var action = OutletActions.Cycle;
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.Expect($"http://{serverAddress}/restapi/relay/outlets/{outletNumber - 1}/cycle/")
                    .Respond(HttpStatusCode.NoContent); // Simulate successful action
            var mockHttpClient = new HttpClient(mockHttp);
            var cts = new CancellationTokenSource();
            cts.Cancel(); // Cancel the token before the request is made
            // Act
            var result = await HttpUtils.TriggerOutletAction(mockHttpClient, serverAddress, outletNumber, action, cts.Token);
            // Assert
            Assert.True(result.IsErr);
            mockHttp.VerifyNoOutstandingExpectation(); // Ensure the request was made
            mockHttp.VerifyNoOutstandingRequest();
        }
    }
}