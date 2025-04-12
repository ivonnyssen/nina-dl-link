using IgorVonNyssen.NINA.DlLink.DlLinkSequenceItems;
using Moq;
using NINA.Core.Model;
using RichardSzalay.MockHttp;
using System.Net;

namespace IgorVonNyssen.NINA.DlLink.Tests {

    public class DlLinkConditionTests {
        private readonly Mock<HttpClient> mockHttpClient = new Mock<HttpClient>();

        [Fact]
        public void Check_ShouldReturnTrue_WhenOutletStateMatchesExpectedState_On() {
            // Arrange
            var mockHttpMessageHandler = new MockHttpMessageHandler();
            var serverAddress = "localhost";
            var outletNumber = 1;

            // Mock the HTTP response for the outlet state
            mockHttpMessageHandler.When($"http://{serverAddress}/restapi/relay/outlets/{outletNumber - 1}/state/")
                .Respond("application/json", "true"); // Simulate the outlet being ON

            var httpClient = new HttpClient(mockHttpMessageHandler);

            var condition = new DlLinkCondition {
                HttpClient = httpClient,
                ServerAddress = serverAddress,
                UserName = "user",
                Password = "password",
                OutletNumber = outletNumber,
                State = OutletStates.On // Expecting the outlet to be ON
            };

            // Act
            var result = condition.Check(null, null);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Check_ShouldReturnFalse_WhenOutletStateDoesNotMatchExpectedState_Off() {
            // Arrange
            var mockHttpMessageHandler = new MockHttpMessageHandler();
            var serverAddress = "localhost";
            var outletNumber = 1;

            // Mock the HTTP response for the outlet state
            mockHttpMessageHandler.When($"http://{serverAddress}/restapi/relay/outlets/{outletNumber - 1}/state/")
                .Respond("application/json", "true"); // Simulate the outlet being ON

            var httpClient = new HttpClient(mockHttpMessageHandler);

            var condition = new DlLinkCondition {
                HttpClient = httpClient,
                ServerAddress = serverAddress,
                UserName = "user",
                Password = "password",
                OutletNumber = outletNumber,
                State = OutletStates.Off // Expecting the outlet to be OFF
            };

            // Act
            var result = condition.Check(null, null);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Check_ShouldThrowException_WhenHttpUtilsFails() {
            // Arrange
            var mockHttpMessageHandler = new MockHttpMessageHandler();
            var serverAddress = "localhost";
            var outletNumber = 1;

            // Mock the HTTP response for the outlet state
            mockHttpMessageHandler.When($"http://{serverAddress}/restapi/relay/outlets/{outletNumber - 1}/state/")
                .Respond(HttpStatusCode.NotFound);

            var httpClient = new HttpClient(mockHttpMessageHandler);

            var condition = new DlLinkCondition {
                HttpClient = httpClient,
                ServerAddress = serverAddress,
                UserName = "user",
                Password = "password",
                OutletNumber = outletNumber,
                State = OutletStates.On
            };

            // Act & Assert
            Assert.Throws<SequenceEntityFailedException>(() => condition.Check(null, null));
        }

        [Fact]
        public void Check_ShouldThrowException_ForInvalidState() {
            // Arrange
            var mockHttpMessageHandler = new MockHttpMessageHandler();
            var serverAddress = "localhost";
            var outletNumber = 1;

            // Mock the HTTP response for the outlet state
            mockHttpMessageHandler.When($"http://{serverAddress}/restapi/relay/outlets/{outletNumber - 1}/state/")
                .Respond("application/json", "true"); // Simulate the outlet being ON

            var httpClient = new HttpClient(mockHttpMessageHandler);

            var condition = new DlLinkCondition {
                HttpClient = httpClient,
                ServerAddress = serverAddress,
                UserName = "user",
                Password = "password",
                OutletNumber = outletNumber,
                State = (OutletStates)999 // Invalid state
            };

            // Act & Assert
            Assert.Throws<SequenceEntityFailedException>(() => condition.Check(null, null));
        }

        [Fact]
        public void Clone_ShouldReturnIdenticalObject() {
            // Arrange
            var condition = new DlLinkCondition {
                OutletNumber = 1,
                State = OutletStates.On
            };

            // Act
            var clone = (DlLinkCondition)condition.Clone();

            // Assert
            Assert.Equal(condition.OutletNumber, clone.OutletNumber);
            Assert.Equal(condition.State, clone.State);
        }

        [Fact]
        public void ToString_ShouldReturnCorrectString() {
            // Arrange
            var condition = new DlLinkCondition {
                OutletNumber = 1,
                State = OutletStates.On,
                Category = "TestCategory"
            };

            // Act
            var result = condition.ToString();

            // Assert
            Assert.Contains("TestCategory", result);
            Assert.Contains("Outlet: 1", result);
            Assert.Contains("State: On", result);
        }

        [Fact]
        public void OutletNumber_ShouldClampNegativeValuesToZero() {
            // Arrange
            var condition = new DlLinkCondition();

            // Act
            condition.OutletNumber = -5;

            // Assert
            Assert.Equal(0, condition.OutletNumber);
        }
    }
}