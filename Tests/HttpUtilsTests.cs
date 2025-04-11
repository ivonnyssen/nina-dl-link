using Xunit;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IgorVonNyssen.NINA.DlLink.DlLinkDrivers;
using IgorVonNyssen.NINA.DlLink.DlLinkSequenceItems;
using System.Collections.Generic;
using RichardSzalay.MockHttp;

namespace IgorVonNyssen.NINA.DlLink.Tests {

    public class HttpUtilsTests {

        [Fact]
        public async Task GetOutletNames_ShouldReturnOutletNames_WhenResponseIsSuccessful() {
            // Arrange
            var serverAddress = "localhost";
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When($"http://{serverAddress}/restapi/relay/outlets/all;/name/")
                    .Respond(HttpStatusCode.MultiStatus, "application/json", "[\"Outlet1\", \"Outlet2\"]");

            var mockHttpClient = new HttpClient(mockHttp);

            // Act
            var result = await HttpUtils.GetOutletNames(mockHttpClient, serverAddress, CancellationToken.None);

            // Assert
            Assert.True(result.IsOk);
            Assert.Equal(new List<string> { "Outlet1", "Outlet2" }, result.Value);
        }

        [Fact]
        public async Task GetOutletNames_ShouldReturnError_WhenResponseIsUnsuccessful() {
            // Arrange
            var serverAddress = "localhost";
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When($"http://{serverAddress}/restapi/relay/outlets/all;/name/")
                    .Respond(HttpStatusCode.BadRequest);

            var mockHttpClient = new HttpClient(mockHttp);

            // Act
            var result = await HttpUtils.GetOutletNames(mockHttpClient, serverAddress, CancellationToken.None);

            // Assert
            Assert.True(result.IsErr);
        }

        [Fact]
        public async Task GetOutletState_ShouldReturnTrue_WhenOutletIsOn() {
            // Arrange
            var serverAddress = "localhost";
            var outletNumber = 1;
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When($"http://{serverAddress}/restapi/relay/outlets/{outletNumber - 1}/state/")
                    .Respond("application/json", "true");

            var mockHttpClient = new HttpClient(mockHttp);

            // Act
            var result = await HttpUtils.GetOutletState(mockHttpClient, serverAddress, outletNumber, CancellationToken.None);

            // Assert
            Assert.True(result.IsOk);
            Assert.True(result.Value);
        }

        [Fact]
        public async Task GetOutletState_ShouldReturnError_WhenOutletNumberIsInvalid() {
            // Arrange
            var serverAddress = "localhost";
            var outletNumber = 0; // Invalid outlet number

            // Act
            var result = await HttpUtils.GetOutletState(httpClient: new HttpClient(), serverAddress, outletNumber, CancellationToken.None);

            // Assert
            Assert.True(result.IsErr);
        }

        [Fact]
        public async Task SetOutletState_ShouldReturnSuccess_WhenStateIsSetSuccessfully() {
            // Arrange
            var serverAddress = "localhost";
            var outletNumber = 1;
            var valueToSet = true;
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When($"http://{serverAddress}/restapi/relay/outlets/{outletNumber - 1}/state/")
                    .Respond(HttpStatusCode.NoContent);

            var mockHttpClient = new HttpClient(mockHttp);

            // Act
            var result = await HttpUtils.SetOutletState(mockHttpClient, serverAddress, outletNumber, valueToSet, CancellationToken.None);

            // Assert
            Assert.True(result.IsOk);
        }

        [Fact]
        public async Task TriggerOutletAction_ShouldReturnSuccess_WhenActionIsCycle() {
            // Arrange
            var serverAddress = "localhost";
            var outletNumber = 1;
            var action = OutletActions.Cycle;
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When($"http://{serverAddress}/restapi/relay/outlets/{outletNumber - 1}/cycle/")
                    .Respond(HttpStatusCode.NoContent);

            var mockHttpClient = new HttpClient(mockHttp);

            // Act
            var result = await HttpUtils.TriggerOutletAction(mockHttpClient, serverAddress, outletNumber, action, CancellationToken.None);

            // Assert
            Assert.True(result.IsOk);
        }

        [Fact]
        public async Task TriggerOutletAction_ShouldReturnError_WhenActionIsInvalid() {
            // Arrange
            var serverAddress = "localhost";
            var outletNumber = 1;
            var action = (OutletActions)999; // Invalid action

            // Act
            var result = await HttpUtils.TriggerOutletAction(new HttpClient(), serverAddress, outletNumber, action, CancellationToken.None);

            // Assert
            Assert.True(result.IsErr);
        }

        [Fact]
        public async Task SetOutletState_ShouldReturnError_WhenOutletNumberIsInvalid() {
            // Arrange
            var serverAddress = "localhost";
            var outletNumber = 0; // Invalid outlet number
            var valueToSet = true;

            // Act
            var result = await HttpUtils.SetOutletState(new HttpClient(), serverAddress, outletNumber, valueToSet, CancellationToken.None);

            // Assert
            Assert.True(result.IsErr);
        }

        [Fact]
        public async Task TriggerOutletAction_ShouldReturnError_WhenOutletNumberIsInvalid() {
            // Arrange
            var serverAddress = "localhost";
            var outletNumber = 0; // Invalid outlet number
            var action = OutletActions.On;

            // Act
            var result = await HttpUtils.TriggerOutletAction(new HttpClient(), serverAddress, outletNumber, action, CancellationToken.None);

            // Assert
            Assert.True(result.IsErr);
        }
    }
}