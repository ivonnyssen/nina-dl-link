using IgorVonNyssen.NINA.DlLink.DlLinkSequenceItems;
using Moq;
using NINA.Equipment.Interfaces.Mediator;

namespace IgorVonNyssen.NINA.DlLink.Tests {

    public class DlLinkRescanTests {
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
        public async Task Execute_ShouldCallCorrectMediator_WhenRescanIsSet(Mediators mediator) {
            // Arrange
            var rescan = new DlLinkRescan(
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
                mockSafetyMonitorMediator.Object) {
                Rescan = mediator,
                Delay = 0
            };

            // Act
            await rescan.Execute(null, CancellationToken.None);

            // Assert
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

                default:
                    Assert.Fail($"Unexpected mediator: {mediator}");
                    break;
            }
        }

        [Fact]
        public async Task Execute_ShouldRespectDelay() {
            // Arrange
            var rescan = new DlLinkRescan(
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
                mockSafetyMonitorMediator.Object) {
                Rescan = Mediators.Camera,
                Delay = 1
            };

            var cancellationTokenSource = new CancellationTokenSource();

            // Act
            var task = rescan.Execute(null, cancellationTokenSource.Token);

            // Assert
            Assert.False(task.IsCompleted);
            await Task.Delay(1500); // Wait for the delay to complete
            Assert.True(task.IsCompleted);
            mockCameraMediator.Verify(m => m.Rescan(), Times.Once);
        }

        [Fact]
        public void Clone_ShouldReturnIdenticalObject() {
            // Arrange
            var rescan = new DlLinkRescan(
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
                mockSafetyMonitorMediator.Object) {
                Rescan = Mediators.Switch,
                Delay = 5
            };

            // Act
            var clone = (DlLinkRescan)rescan.Clone();

            // Assert
            Assert.Equal(rescan.Rescan, clone.Rescan);
            Assert.Equal(rescan.Delay, clone.Delay);
        }

        [Fact]
        public void ToString_ShouldReturnCorrectString() {
            // Arrange
            var rescan = new DlLinkRescan(
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
                mockSafetyMonitorMediator.Object) {
                Rescan = Mediators.Telescope
            };

            // Act
            var result = rescan.ToString();

            // Assert
            Assert.Contains("Telescope", result);
        }

        [Fact]
        public async Task Execute_ShouldTreatNegativeDelayAsZero() {
            // Arrange
            var rescan = new DlLinkRescan(
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
                mockSafetyMonitorMediator.Object) {
                Rescan = Mediators.Camera,
                Delay = -5 // Set a negative delay
            };

            // Act
            await rescan.Execute(null, CancellationToken.None);

            // Assert
            Assert.Equal(0, rescan.Delay); // Ensure the delay is clamped to 0
            mockCameraMediator.Verify(m => m.Rescan(), Times.Once); // Ensure the Rescan method is called immediately
        }
    }
}