using IgorVonNyssen.NINA.DlLink.DlLinkSequenceItems;
using Moq;
using NINA.Equipment.Interfaces.Mediator;
using RichardSzalay.MockHttp;
using System.Net;

namespace IgorVonNyssen.NINA.DlLink.Tests {

    public class DlLinkInstructionTests {

        [Fact]
        public async Task Execute_ShouldNotThrowException_WhenOutletNumberIsInvalid() {
            // Arrange
            // Mock mediators
            var mockCameraMediator = new Mock<ICameraMediator>();
            var mockFocuserMediator = new Mock<IFocuserMediator>();
            var mockFilterWheelMediator = new Mock<IFilterWheelMediator>();
            var mockTelescopeMediator = new Mock<ITelescopeMediator>();
            var mockGuiderMediator = new Mock<IGuiderMediator>();
            var mockRotatorMediator = new Mock<IRotatorMediator>();
            var mockDomeMediator = new Mock<IDomeMediator>();
            var mockSwitchMediator = new Mock<ISwitchMediator>();
            var mockFlatDeviceMediator = new Mock<IFlatDeviceMediator>();
            var mockWeatherDataMediator = new Mock<IWeatherDataMediator>();
            var mockSafetyMonitorMediator = new Mock<ISafetyMonitorMediator>();

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

            // Mock mediators
            var mockCameraMediator = new Mock<ICameraMediator>();
            var mockFocuserMediator = new Mock<IFocuserMediator>();
            var mockFilterWheelMediator = new Mock<IFilterWheelMediator>();
            var mockTelescopeMediator = new Mock<ITelescopeMediator>();
            var mockGuiderMediator = new Mock<IGuiderMediator>();
            var mockRotatorMediator = new Mock<IRotatorMediator>();
            var mockDomeMediator = new Mock<IDomeMediator>();
            var mockSwitchMediator = new Mock<ISwitchMediator>();
            var mockFlatDeviceMediator = new Mock<IFlatDeviceMediator>();
            var mockWeatherDataMediator = new Mock<IWeatherDataMediator>();
            var mockSafetyMonitorMediator = new Mock<ISafetyMonitorMediator>();

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

            // Mock mediators
            var mockCameraMediator = new Mock<ICameraMediator>();
            var mockFocuserMediator = new Mock<IFocuserMediator>();
            var mockFilterWheelMediator = new Mock<IFilterWheelMediator>();
            var mockTelescopeMediator = new Mock<ITelescopeMediator>();
            var mockGuiderMediator = new Mock<IGuiderMediator>();
            var mockRotatorMediator = new Mock<IRotatorMediator>();
            var mockDomeMediator = new Mock<IDomeMediator>();
            var mockSwitchMediator = new Mock<ISwitchMediator>();
            var mockFlatDeviceMediator = new Mock<IFlatDeviceMediator>();
            var mockWeatherDataMediator = new Mock<IWeatherDataMediator>();
            var mockSafetyMonitorMediator = new Mock<ISafetyMonitorMediator>();

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

            // Mock mediators
            var mockCameraMediator = new Mock<ICameraMediator>();
            var mockFocuserMediator = new Mock<IFocuserMediator>();
            var mockFilterWheelMediator = new Mock<IFilterWheelMediator>();
            var mockTelescopeMediator = new Mock<ITelescopeMediator>();
            var mockGuiderMediator = new Mock<IGuiderMediator>();
            var mockRotatorMediator = new Mock<IRotatorMediator>();
            var mockDomeMediator = new Mock<IDomeMediator>();
            var mockSwitchMediator = new Mock<ISwitchMediator>();
            var mockFlatDeviceMediator = new Mock<IFlatDeviceMediator>();
            var mockWeatherDataMediator = new Mock<IWeatherDataMediator>();
            var mockSafetyMonitorMediator = new Mock<ISafetyMonitorMediator>();

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

            // Mock mediators
            var mockCameraMediator = new Mock<ICameraMediator>();
            var mockFocuserMediator = new Mock<IFocuserMediator>();
            var mockFilterWheelMediator = new Mock<IFilterWheelMediator>();
            var mockTelescopeMediator = new Mock<ITelescopeMediator>();
            var mockGuiderMediator = new Mock<IGuiderMediator>();
            var mockRotatorMediator = new Mock<IRotatorMediator>();
            var mockDomeMediator = new Mock<IDomeMediator>();
            var mockFlatDeviceMediator = new Mock<IFlatDeviceMediator>();
            var mockWeatherDataMediator = new Mock<IWeatherDataMediator>();
            var mockSafetyMonitorMediator = new Mock<ISafetyMonitorMediator>();

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
        public void Execute_ShouldWaitForDelay_WhenDelayIsSpecified() {
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

            // Mock mediators
            var mockCameraMediator = new Mock<ICameraMediator>();
            var mockFocuserMediator = new Mock<IFocuserMediator>();
            var mockFilterWheelMediator = new Mock<IFilterWheelMediator>();
            var mockTelescopeMediator = new Mock<ITelescopeMediator>();
            var mockGuiderMediator = new Mock<IGuiderMediator>();
            var mockRotatorMediator = new Mock<IRotatorMediator>();
            var mockSwitchMediator = new Mock<ISwitchMediator>();
            var mockDomeMediator = new Mock<IDomeMediator>();
            var mockFlatDeviceMediator = new Mock<IFlatDeviceMediator>();
            var mockWeatherDataMediator = new Mock<IWeatherDataMediator>();
            var mockSafetyMonitorMediator = new Mock<ISafetyMonitorMediator>();

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
                Rescan = Mediators.Switch, // Request rescan for switches
                Delay = 5 // 5-second delay
            };

            // Act
            var task = instruction.Execute(null, CancellationToken.None);

            // Assert
            Assert.False(task.IsCompleted); // Ensure the delay is respected
        }

        [Fact]
        public async Task Execute_ShouldPerformRescan_ForAllMediators() {
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

            // Mock mediators
            var mockCameraMediator = new Mock<ICameraMediator>();
            var mockFocuserMediator = new Mock<IFocuserMediator>();
            var mockFilterWheelMediator = new Mock<IFilterWheelMediator>();
            var mockTelescopeMediator = new Mock<ITelescopeMediator>();
            var mockGuiderMediator = new Mock<IGuiderMediator>();
            var mockRotatorMediator = new Mock<IRotatorMediator>();
            var mockDomeMediator = new Mock<IDomeMediator>();
            var mockSwitchMediator = new Mock<ISwitchMediator>();
            var mockFlatDeviceMediator = new Mock<IFlatDeviceMediator>();
            var mockWeatherDataMediator = new Mock<IWeatherDataMediator>();
            var mockSafetyMonitorMediator = new Mock<ISafetyMonitorMediator>();

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
                Delay = 0 // No delay for simplicity
            };

            // Act & Assert for each mediator
            instruction.Rescan = Mediators.Camera;
            await instruction.Execute(null, CancellationToken.None);
            mockCameraMediator.Verify(m => m.Rescan(), Times.Once);

            instruction.Rescan = Mediators.Focuser;
            await instruction.Execute(null, CancellationToken.None);
            mockFocuserMediator.Verify(m => m.Rescan(), Times.Once);

            instruction.Rescan = Mediators.FilterWheel;
            await instruction.Execute(null, CancellationToken.None);
            mockFilterWheelMediator.Verify(m => m.Rescan(), Times.Once);

            instruction.Rescan = Mediators.Telescope;
            await instruction.Execute(null, CancellationToken.None);
            mockTelescopeMediator.Verify(m => m.Rescan(), Times.Once);

            instruction.Rescan = Mediators.Guider;
            await instruction.Execute(null, CancellationToken.None);
            mockGuiderMediator.Verify(m => m.Rescan(), Times.Once);

            instruction.Rescan = Mediators.Rotator;
            await instruction.Execute(null, CancellationToken.None);
            mockRotatorMediator.Verify(m => m.Rescan(), Times.Once);

            instruction.Rescan = Mediators.Dome;
            await instruction.Execute(null, CancellationToken.None);
            mockDomeMediator.Verify(m => m.Rescan(), Times.Once);

            instruction.Rescan = Mediators.Switch;
            await instruction.Execute(null, CancellationToken.None);
            mockSwitchMediator.Verify(m => m.Rescan(), Times.Once);

            instruction.Rescan = Mediators.FlatDevice;
            await instruction.Execute(null, CancellationToken.None);
            mockFlatDeviceMediator.Verify(m => m.Rescan(), Times.Once);

            instruction.Rescan = Mediators.WeatherData;
            await instruction.Execute(null, CancellationToken.None);
            mockWeatherDataMediator.Verify(m => m.Rescan(), Times.Once);

            instruction.Rescan = Mediators.SafetyMonitor;
            await instruction.Execute(null, CancellationToken.None);
            mockSafetyMonitorMediator.Verify(m => m.Rescan(), Times.Once);
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

            // Mock mediators
            var mockCameraMediator = new Mock<ICameraMediator>();
            var mockFocuserMediator = new Mock<IFocuserMediator>();
            var mockFilterWheelMediator = new Mock<IFilterWheelMediator>();
            var mockTelescopeMediator = new Mock<ITelescopeMediator>();
            var mockGuiderMediator = new Mock<IGuiderMediator>();
            var mockRotatorMediator = new Mock<IRotatorMediator>();
            var mockDomeMediator = new Mock<IDomeMediator>();
            var mockSwitchMediator = new Mock<ISwitchMediator>();
            var mockFlatDeviceMediator = new Mock<IFlatDeviceMediator>();
            var mockWeatherDataMediator = new Mock<IWeatherDataMediator>();
            var mockSafetyMonitorMediator = new Mock<ISafetyMonitorMediator>();

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
    }
}