using IgorVonNyssen.NINA.DlLink.DlLinkSequenceItems;
using Moq;
using NINA.Equipment.Interfaces.Mediator;
using RichardSzalay.MockHttp;
using System.Net;

namespace IgorVonNyssen.NINA.DlLink.Tests {

    public class DlLinkInstructionTests {
        private readonly Mock<ICameraMediator> mockCameraMediator = new();
        private readonly Mock<IFocuserMediator> mockFocuserMediator = new();
        private readonly Mock<IFilterWheelMediator> mockFilterWheelMediator = new();
        private readonly Mock<ITelescopeMediator> mockTelescopeMediator = new();
        private readonly Mock<IGuiderMediator> mockGuiderMediator = new();
        private readonly Mock<IRotatorMediator> mockRotatorMediator = new();
        private readonly Mock<IDomeMediator> mockDomeMediator = new();
        private readonly Mock<ISwitchMediator> mockSwitchMediator = new();
        private readonly Mock<IFlatDeviceMediator> mockFlatDeviceMediator = new();
        private readonly Mock<IWeatherDataMediator> mockWeatherDataMediator = new();
        private readonly Mock<ISafetyMonitorMediator> mockSafetyMonitorMediator = new();

        [Fact]
        public async Task Execute_ShouldNotThrowException_WhenOutletNumberIsInvalid() {
            // Arrange
            // Create an instruction with all mediators injected
            var instruction = new DlLinkInstruction(
                mockCameraMediator.Object,
                mockFocuserMediator.Object,
                mockFilterWheelMediator.Object,
                mockTelescopeMediator.Object,
                mockGuiderMediator.Object,
                mockRotatorMediator.Object,
                mockDomeMediator.Object,
                mockSwitchMediator.Object,
                mockFlatDeviceMediator.Object,
                mockWeatherDataMediator.Object,
                mockSafetyMonitorMediator.Object
            ) {
                OutletNumber = -1 // Invalid outlet number
            };

            // Act
            await instruction.Execute(null, CancellationToken.None);
        }

        [Fact]
        public async Task Execute_ShouldNothTrowException_WhenGetOutletStateFails() {
            // Arrange
            var mockHttpMessageHandler = new MockHttpMessageHandler();
            var serverAddress = "localhost";
            var outletNumber = 1;

            mockHttpMessageHandler.Expect($"http://{serverAddress}/restapi/relay/outlets/{outletNumber - 1}/state/")
                .Respond(HttpStatusCode.InternalServerError); // Simulate failure

            var httpClient = new HttpClient(mockHttpMessageHandler);

            // Create an instruction with all mediators injected
            var instruction = new DlLinkInstruction(
                mockCameraMediator.Object,
                mockFocuserMediator.Object,
                mockFilterWheelMediator.Object,
                mockTelescopeMediator.Object,
                mockGuiderMediator.Object,
                mockRotatorMediator.Object,
                mockDomeMediator.Object,
                mockSwitchMediator.Object,
                mockFlatDeviceMediator.Object,
                mockWeatherDataMediator.Object,
                mockSafetyMonitorMediator.Object
            ) {
                HttpClient = httpClient,
                ServerAddress = serverAddress,
                OutletNumber = outletNumber
            };

            // Act
            await instruction.Execute(null, CancellationToken.None);

            // Assert
            mockHttpMessageHandler.VerifyNoOutstandingExpectation(); // Ensure no unexpected requests were made
            mockHttpMessageHandler.VerifyNoOutstandingRequest();
        }

        [Fact]
        public async Task Execute_ShouldSkipAction_WhenOutletIsAlreadyInDesiredState() {
            // Arrange
            var mockHttpMessageHandler = new MockHttpMessageHandler();
            var serverAddress = "localhost";
            var outletNumber = 1;

            mockHttpMessageHandler.Expect($"http://{serverAddress}/restapi/relay/outlets/{outletNumber - 1}/state/")
                .Respond("application/json", "true"); // Simulate outlet is already ON

            var httpClient = new HttpClient(mockHttpMessageHandler);

            // Create an instruction with all mediators injected
            var instruction = new DlLinkInstruction(
                mockCameraMediator.Object,
                mockFocuserMediator.Object,
                mockFilterWheelMediator.Object,
                mockTelescopeMediator.Object,
                mockGuiderMediator.Object,
                mockRotatorMediator.Object,
                mockDomeMediator.Object,
                mockSwitchMediator.Object,
                mockFlatDeviceMediator.Object,
                mockWeatherDataMediator.Object,
                mockSafetyMonitorMediator.Object
            ) {
                HttpClient = httpClient,
                ServerAddress = serverAddress,
                OutletNumber = outletNumber,
                Action = OutletActions.On // Desired state is ON
            };

            // Act
            await instruction.Execute(null, CancellationToken.None);

            // Assert
            mockHttpMessageHandler.VerifyNoOutstandingExpectation(); // Ensure no unexpected requests were made
            mockHttpMessageHandler.VerifyNoOutstandingRequest();
        }

        [Fact]
        public async Task Execute_ShouldTriggerAction_WhenOutletIsNotInDesiredState() {
            // Arrange
            var mockHttpMessageHandler = new MockHttpMessageHandler();
            var serverAddress = "localhost";
            var outletNumber = 1;

            mockHttpMessageHandler.Expect($"http://{serverAddress}/restapi/relay/outlets/{outletNumber - 1}/state/")
                .Respond("application/json", "false"); // Simulate outlet is OFF

            mockHttpMessageHandler.Expect(HttpMethod.Put, $"http://{serverAddress}/restapi/relay/outlets/{outletNumber - 1}/state/")
                .Respond(HttpStatusCode.NoContent); // Simulate successful action

            var httpClient = new HttpClient(mockHttpMessageHandler);

            // Create an instruction with all mediators injected
            var instruction = new DlLinkInstruction(
                mockCameraMediator.Object,
                mockFocuserMediator.Object,
                mockFilterWheelMediator.Object,
                mockTelescopeMediator.Object,
                mockGuiderMediator.Object,
                mockRotatorMediator.Object,
                mockDomeMediator.Object,
                mockSwitchMediator.Object,
                mockFlatDeviceMediator.Object,
                mockWeatherDataMediator.Object,
                mockSafetyMonitorMediator.Object
            ) {
                HttpClient = httpClient,
                ServerAddress = serverAddress,
                OutletNumber = outletNumber,
                Action = OutletActions.On // Desired state is ON
            };

            // Act
            await instruction.Execute(null, CancellationToken.None);

            // Assert
            mockHttpMessageHandler.VerifyNoOutstandingExpectation(); // Ensure no unexpected requests were made
            mockHttpMessageHandler.VerifyNoOutstandingRequest();
        }

        [Fact]
        public async Task Execute_ShouldPerformRescan_WhenRescanIsRequested() {
            // Arrange
            var mockHttpMessageHandler = new MockHttpMessageHandler();
            var serverAddress = "localhost";
            var outletNumber = 1;

            // Mock the HTTP response for the outlet state
            mockHttpMessageHandler.Expect($"http://{serverAddress}/restapi/relay/outlets/{outletNumber - 1}/state/")
                .Respond("application/json", "false"); // Simulate the outlet being OFF

            mockHttpMessageHandler.Expect($"http://{serverAddress}/restapi/relay/outlets/{outletNumber - 1}/state/")
                .Respond(HttpStatusCode.NoContent); // Simulate successful action

            var mockSwitchMediator = new Mock<ISwitchMediator>();

            var httpClient = new HttpClient(mockHttpMessageHandler);

            // Create an instruction with all mediators injected
            var instruction = new DlLinkInstruction(
                mockCameraMediator.Object,
                mockFocuserMediator.Object,
                mockFilterWheelMediator.Object,
                mockTelescopeMediator.Object,
                mockGuiderMediator.Object,
                mockRotatorMediator.Object,
                mockDomeMediator.Object,
                mockSwitchMediator.Object,
                mockFlatDeviceMediator.Object,
                mockWeatherDataMediator.Object,
                mockSafetyMonitorMediator.Object
            ) {
                HttpClient = httpClient,
                ServerAddress = serverAddress,
                OutletNumber = outletNumber,
                Action = OutletActions.On, // Desired state is ON
                Rescan = Mediators.Switch // Request rescan for switches
            };

            // Act
            await instruction.Execute(null, CancellationToken.None);

            // Assert
            mockHttpMessageHandler.VerifyNoOutstandingExpectation(); // Ensure no unexpected requests were made
            mockHttpMessageHandler.VerifyNoOutstandingRequest();
            mockSwitchMediator.Verify(m => m.Rescan(), Times.Once);
        }

        [Fact]
        public async Task Execute_ShouldWaitForDelay_WhenDelayIsSpecified() {
            // Arrange
            var mockHttpMessageHandler = new MockHttpMessageHandler();
            var serverAddress = "localhost";
            var outletNumber = 1;

            // Mock the HTTP response for the outlet state
            mockHttpMessageHandler.When($"http://{serverAddress}/restapi/relay/outlets/{outletNumber - 1}/state/")
                .Respond("application/json", "false"); // Simulate outlet is OFF

            mockHttpMessageHandler.When(HttpMethod.Put, $"http://{serverAddress}/restapi/relay/outlets/{outletNumber - 1}/state/")
                .Respond(HttpStatusCode.NoContent); // Simulate successful action

            var httpClient = new HttpClient(mockHttpMessageHandler);

            // Create an instruction with all mediators injected
            var instruction = new DlLinkInstruction(
                mockCameraMediator.Object,
                mockFocuserMediator.Object,
                mockFilterWheelMediator.Object,
                mockTelescopeMediator.Object,
                mockGuiderMediator.Object,
                mockRotatorMediator.Object,
                mockDomeMediator.Object,
                mockSwitchMediator.Object,
                mockFlatDeviceMediator.Object,
                mockWeatherDataMediator.Object,
                mockSafetyMonitorMediator.Object
            ) {
                HttpClient = httpClient,
                ServerAddress = serverAddress,
                OutletNumber = outletNumber,
                Action = OutletActions.On, // Desired state is ON
                Rescan = Mediators.Camera, // Request rescan for camera
                Delay = 1 // 1-second delay
            };

            // Act
            var task = instruction.Execute(null, CancellationToken.None);

            // Assert
            Assert.False(task.IsCompleted); // Ensure the delay is respected
            await Task.Delay(1500); // Wait for the delay to complete
            Assert.True(task.IsCompleted);
            mockCameraMediator.Verify(m => m.Rescan(), Times.Once);
        }

        [Theory]
        [InlineData(Mediators.Camera)]
        [InlineData(Mediators.Focuser)]
        [InlineData(Mediators.FilterWheel)]
        [InlineData(Mediators.Telescope)]
        [InlineData(Mediators.Guider)]
        [InlineData(Mediators.Rotator)]
        [InlineData(Mediators.Dome)]
        [InlineData(Mediators.Switch)]
        [InlineData(Mediators.FlatDevice)]
        [InlineData(Mediators.WeatherData)]
        [InlineData(Mediators.SafetyMonitor)]
        [InlineData(Mediators.None)]
        [InlineData((Mediators)999)]
        public async Task Execute_ShouldPerformRescan_ForAllMediators(Mediators mediator) {
            // Arrange
            var mockHttpMessageHandler = new MockHttpMessageHandler();
            var serverAddress = "localhost";
            var outletNumber = 1;

            // Mock the HTTP response for the outlet state
            mockHttpMessageHandler.When($"http://{serverAddress}/restapi/relay/outlets/{outletNumber - 1}/state/")
                .Respond("application/json", "false"); // Simulate the outlet being OFF

            mockHttpMessageHandler.When($"http://{serverAddress}/restapi/relay/outlets/{outletNumber - 1}/state/")
                .Respond(HttpStatusCode.NoContent); // Simulate successful action

            var httpClient = new HttpClient(mockHttpMessageHandler);

            // Create an instruction with all mediators injected
            var instruction = new DlLinkInstruction(
                mockCameraMediator.Object,
                mockFocuserMediator.Object,
                mockFilterWheelMediator.Object,
                mockTelescopeMediator.Object,
                mockGuiderMediator.Object,
                mockRotatorMediator.Object,
                mockDomeMediator.Object,
                mockSwitchMediator.Object,
                mockFlatDeviceMediator.Object,
                mockWeatherDataMediator.Object,
                mockSafetyMonitorMediator.Object
            ) {
                HttpClient = httpClient,
                ServerAddress = serverAddress,
                OutletNumber = outletNumber,
                Action = OutletActions.On, // Desired state is ON
                Rescan = mediator, // Request rescan for the specified mediator
                Delay = 0 // No delay for simplicity
            };

            // Act
            await instruction.Execute(null, CancellationToken.None);

            switch (mediator) {
                case Mediators.Camera:
                    mockCameraMediator.Verify(m => m.Rescan(), Times.Once);
                    break;

                case Mediators.Focuser:
                    mockFocuserMediator.Verify(m => m.Rescan(), Times.Once);
                    break;

                case Mediators.FilterWheel:
                    mockFilterWheelMediator.Verify(m => m.Rescan(), Times.Once);
                    break;

                case Mediators.Telescope:
                    mockTelescopeMediator.Verify(m => m.Rescan(), Times.Once);
                    break;

                case Mediators.Guider:
                    mockGuiderMediator.Verify(m => m.Rescan(), Times.Once);
                    break;

                case Mediators.Rotator:
                    mockRotatorMediator.Verify(m => m.Rescan(), Times.Once);
                    break;

                case Mediators.Dome:
                    mockDomeMediator.Verify(m => m.Rescan(), Times.Once);
                    break;

                case Mediators.Switch:
                    mockSwitchMediator.Verify(m => m.Rescan(), Times.Once);
                    break;

                case Mediators.FlatDevice:
                    mockFlatDeviceMediator.Verify(m => m.Rescan(), Times.Once);
                    break;

                case Mediators.WeatherData:
                    mockWeatherDataMediator.Verify(m => m.Rescan(), Times.Once);
                    break;

                case Mediators.SafetyMonitor:
                    mockSafetyMonitorMediator.Verify(m => m.Rescan(), Times.Once);
                    break;

                case Mediators.None:
                    mockCameraMediator.Verify(m => m.Rescan(), Times.Never);
                    mockFocuserMediator.Verify(m => m.Rescan(), Times.Never);
                    mockFilterWheelMediator.Verify(m => m.Rescan(), Times.Never);
                    mockTelescopeMediator.Verify(m => m.Rescan(), Times.Never);
                    mockGuiderMediator.Verify(m => m.Rescan(), Times.Never);
                    mockRotatorMediator.Verify(m => m.Rescan(), Times.Never);
                    mockDomeMediator.Verify(m => m.Rescan(), Times.Never);
                    mockSwitchMediator.Verify(m => m.Rescan(), Times.Never);
                    mockFlatDeviceMediator.Verify(m => m.Rescan(), Times.Never);
                    mockWeatherDataMediator.Verify(m => m.Rescan(), Times.Never);
                    mockSafetyMonitorMediator.Verify(m => m.Rescan(), Times.Never);
                    break;

                case (Mediators)999:
                    // This case is for an unexpected mediator value
                    // it should not touch any of the mediators
                    mockCameraMediator.Verify(m => m.Rescan(), Times.Never);
                    mockFocuserMediator.Verify(m => m.Rescan(), Times.Never);
                    mockFilterWheelMediator.Verify(m => m.Rescan(), Times.Never);
                    mockTelescopeMediator.Verify(m => m.Rescan(), Times.Never);
                    mockGuiderMediator.Verify(m => m.Rescan(), Times.Never);
                    mockRotatorMediator.Verify(m => m.Rescan(), Times.Never);
                    mockDomeMediator.Verify(m => m.Rescan(), Times.Never);
                    mockSwitchMediator.Verify(m => m.Rescan(), Times.Never);
                    mockFlatDeviceMediator.Verify(m => m.Rescan(), Times.Never);
                    mockWeatherDataMediator.Verify(m => m.Rescan(), Times.Never);
                    mockSafetyMonitorMediator.Verify(m => m.Rescan(), Times.Never);
                    break;

                default:
                    Assert.Fail($"Unexpected mediator: {mediator}"); // Fail the test if an unexpected mediator is encountered
                    break;
            }
        }

        [Fact]
        public async Task Execute_ShouldLogError_WhenOutletStateIsInvalid() {
            // Arrange
            var mockHttpMessageHandler = new MockHttpMessageHandler();
            var serverAddress = "localhost";
            var outletNumber = 1;

            // Mock the HTTP response for the outlet state
            mockHttpMessageHandler.Expect($"http://{serverAddress}/restapi/relay/outlets/{outletNumber - 1}/state/")
                .Respond("application/json", "invalid_state"); // Simulate an invalid outlet state

            var httpClient = new HttpClient(mockHttpMessageHandler);

            // Create an instruction with all mediators injected
            var instruction = new DlLinkInstruction(
                mockCameraMediator.Object,
                mockFocuserMediator.Object,
                mockFilterWheelMediator.Object,
                mockTelescopeMediator.Object,
                mockGuiderMediator.Object,
                mockRotatorMediator.Object,
                mockDomeMediator.Object,
                mockSwitchMediator.Object,
                mockFlatDeviceMediator.Object,
                mockWeatherDataMediator.Object,
                mockSafetyMonitorMediator.Object
            ) {
                HttpClient = httpClient,
                ServerAddress = serverAddress,
                OutletNumber = outletNumber,
                Action = OutletActions.On // Desired state is ON
            };

            // Act
            await instruction.Execute(null, CancellationToken.None);

            // Assert
            mockHttpMessageHandler.VerifyNoOutstandingExpectation(); // Ensure no unexpected requests were made
            mockHttpMessageHandler.VerifyNoOutstandingRequest();
        }

        [Fact]
        public void Clone_ShouldReturnIdenticalObject() {
            // Arrange
            var instruction = new DlLinkInstruction(
                mockCameraMediator.Object,
                mockFocuserMediator.Object,
                mockFilterWheelMediator.Object,
                mockTelescopeMediator.Object,
                mockGuiderMediator.Object,
                mockRotatorMediator.Object,
                mockDomeMediator.Object,
                mockSwitchMediator.Object,
                mockFlatDeviceMediator.Object,
                mockWeatherDataMediator.Object,
                mockSafetyMonitorMediator.Object
            ) {
                OutletNumber = 5,
                Action = OutletActions.On,
                Delay = 10,
                Rescan = Mediators.Camera
            };

            // Act
            var clone = (DlLinkInstruction)instruction.Clone();

            // Assert
            Assert.NotNull(clone);
            Assert.NotSame(instruction, clone); // Ensure it's a different instance
            Assert.Equal(instruction.OutletNumber, clone.OutletNumber);
            Assert.Equal(instruction.Action, clone.Action);
            Assert.Equal(instruction.Delay, clone.Delay);
            Assert.Equal(instruction.Rescan, clone.Rescan);
        }

        [Fact]
        public void ToString_ShouldReturnCorrectString() {
            // Arrange
            var instruction = new DlLinkInstruction(
                mockCameraMediator.Object,
                mockFocuserMediator.Object,
                mockFilterWheelMediator.Object,
                mockTelescopeMediator.Object,
                mockGuiderMediator.Object,
                mockRotatorMediator.Object,
                mockDomeMediator.Object,
                mockSwitchMediator.Object,
                mockFlatDeviceMediator.Object,
                mockWeatherDataMediator.Object,
                mockSafetyMonitorMediator.Object
            ) {
                OutletNumber = 3,
                Action = OutletActions.Cycle,
                Delay = 5,
                Rescan = Mediators.Telescope
            };

            // Act
            var result = instruction.ToString();

            // Assert
            Assert.Contains("Category:", result);
            Assert.Contains("Item: DlLinkInstruction", result);
            Assert.Contains("Outlet: 3", result);
            Assert.Contains("Action: Cycle", result);
            Assert.Contains("Delay: 5", result);
            Assert.Contains("Rescan: Telescope", result);
        }
    }
}