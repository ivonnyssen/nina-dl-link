using IgorVonNyssen.NINA.DlLink.DlLinkDrivers;
using Moq;
using NINA.Equipment.Interfaces;
using RichardSzalay.MockHttp;
using System.Net;

namespace IgorVonNyssen.NINA.DlLink.Tests {

    public class DlLinkDriverTests {

        [Fact]
        public async Task Connect_ShouldReturnTrue_WhenResponseIsSuccessful() {
            // Arrange
            var serverAddress = "localhost";
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.Expect($"http://{serverAddress}/restapi/relay/outlets/all;/name/")
                    .Respond(HttpStatusCode.MultiStatus, "application/json", "[\"Outlet1\", \"Outlet2\"]");

            var mockHttpClient = new HttpClient(mockHttp);

            var dlLinkDriver = new DlLinkDriver("TestDevice", mockHttpClient, "localhost");

            // Act
            var result = await dlLinkDriver.Connect(CancellationToken.None);

            // Assert
            Assert.True(result);
            Assert.True(dlLinkDriver.Connected);
            Assert.Equal(2, ((ISwitchHub)dlLinkDriver).Switches.Count);
            Assert.Equal("Outlet1", ((ISwitchHub)dlLinkDriver).Switches.ElementAt(0).Name);
            Assert.Equal(1, ((DlOutlet)((ISwitchHub)dlLinkDriver).Switches.ElementAt(0)).OutletNumber);
            Assert.Equal("Outlet2", ((ISwitchHub)dlLinkDriver).Switches.ElementAt(1).Name);
            Assert.Equal(2, ((DlOutlet)((ISwitchHub)dlLinkDriver).Switches.ElementAt(1)).OutletNumber);
            mockHttp.VerifyNoOutstandingExpectation(); // Ensure the request was made
            mockHttp.VerifyNoOutstandingRequest();
        }

        [Fact]
        public async Task Connect_ShouldReturnFalse_WhenResponseIsUnsuccessful() {
            // Arrange
            var serverAddress = "localhost";
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.Expect($"http://{serverAddress}/restapi/relay/outlets/all;/name/")
                    .Respond(HttpStatusCode.BadRequest);

            var mockHttpClient = new HttpClient(mockHttp);

            var dlLinkDriver = new DlLinkDriver("TestDevice", mockHttpClient, "localhost");

            // Act
            var result = await dlLinkDriver.Connect(CancellationToken.None);

            // Assert
            Assert.False(result);
            Assert.False(dlLinkDriver.Connected);
            Assert.Empty(((ISwitchHub)dlLinkDriver).Switches);
            mockHttp.VerifyNoOutstandingExpectation(); // Ensure the request was made
            mockHttp.VerifyNoOutstandingRequest();
        }

        [Fact]
        public async Task Connect_ShouldReturnWithoutOutlets_WhenResponseIsEmpty() {
            // Arrange
            var serverAddress = "localhost";
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.Expect($"http://{serverAddress}/restapi/relay/outlets/all;/name/")
                    .Respond(HttpStatusCode.MultiStatus, "application/json", "[]");

            var httpClient = new HttpClient(mockHttp);

            var dlLinkDriver = new DlLinkDriver("TestDevice", httpClient, "localhost");

            // Act
            var result = await dlLinkDriver.Connect(CancellationToken.None);

            // Assert
            Assert.True(result);
            Assert.True(dlLinkDriver.Connected);
            Assert.Empty(((ISwitchHub)dlLinkDriver).Switches);
            mockHttp.VerifyNoOutstandingExpectation(); // Ensure the request was made
            mockHttp.VerifyNoOutstandingRequest();
        }

        [Fact]
        public async Task Connect_ShouldReturnFalse_WhenResponseIsInvalidJson() {
            // Arrange
            var serverAddress = "localhost";
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.Expect($"http://{serverAddress}/restapi/relay/outlets/all;/name/")
                    .Respond(HttpStatusCode.MultiStatus, "application/json", "");

            var httpClient = new HttpClient(mockHttp);

            var dlLinkDriver = new DlLinkDriver("TestDevice", httpClient, "localhost");

            // Act
            var result = await dlLinkDriver.Connect(CancellationToken.None);

            // Assert
            Assert.False(result);
            Assert.False(dlLinkDriver.Connected);
            Assert.Empty(((ISwitchHub)dlLinkDriver).Switches);
            mockHttp.VerifyNoOutstandingExpectation(); // Ensure the request was made
            mockHttp.VerifyNoOutstandingRequest();
        }

        [Fact]
        public async Task Connect_ShouldReturnFalse_WhenServerTimesOut() {
            // Arrange
            var serverAddress = "localhost";
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.Expect($"http://{serverAddress}/restapi/relay/outlets/all;/name/")
                    .Throw(new TaskCanceledException());

            var httpClient = new HttpClient(mockHttp);

            var dlLinkDriver = new DlLinkDriver("TestDevice", httpClient, "localhost");

            // Act
            var result = await dlLinkDriver.Connect(CancellationToken.None);

            // Assert
            Assert.False(result);
            Assert.False(dlLinkDriver.Connected);
            Assert.Empty(((ISwitchHub)dlLinkDriver).Switches);
            mockHttp.VerifyNoOutstandingExpectation(); // Ensure the request was made
            mockHttp.VerifyNoOutstandingRequest();
        }

        [Fact]
        public void Disconnect_ShouldClearSwitches() {
            // Arrange
            var mockHttpClient = new Mock<HttpClient>();
            var dlLinkDriver = new DlLinkDriver("TestDevice", mockHttpClient.Object, "localhost");

            // Simulate adding switches to the collection
            var switches = (ICollection<ISwitch>)((ISwitchHub)dlLinkDriver).Switches;
            switches.Add(new DlOutlet("Outlet1", 0));
            switches.Add(new DlOutlet("Outlet2", 1));

            Assert.Equal(2, switches.Count); // Ensure switches are added

            // Act
            dlLinkDriver.Disconnect();

            // Assert
            Assert.Empty(switches); // Ensure switches are cleared
        }
    }
}