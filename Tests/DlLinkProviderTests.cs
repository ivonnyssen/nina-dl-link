using IgorVonNyssen.NINA.DlLink.DlLinkDrivers;
using Moq;
using NINA.Profile.Interfaces;
using System.Reflection;

namespace IgorVonNyssen.NINA.DlLink.Tests {

    public class DlLinkProviderTests {

        [Fact]
        public void GetEquipment_ShouldReturnDlLinkDriver() {
            // Arrange
            var mockProfileService = new Mock<IProfileService>();
            var provider = new DlLinkProvider(mockProfileService.Object);

            // Use reflection to set the ShowSwitchHub property
            var settingsType = typeof(Properties.Settings);
            var defaultInstance = settingsType.GetProperty("Default", BindingFlags.Static | BindingFlags.Public)?.GetValue(null);
            var showSwitchHubProperty = defaultInstance?.GetType().GetProperty("ShowSwitchHub", BindingFlags.Instance | BindingFlags.Public);
            showSwitchHubProperty?.SetValue(defaultInstance, true);
            // Act
            var equipment = provider.GetEquipment();

            // Assert
            Assert.NotNull(equipment);
            Assert.Single(equipment); // Ensure only one device is returned
            Assert.IsType<DlLinkDriver>(equipment[0]); // Ensure the device is a DlLinkDriver
            Assert.Equal("DL Link", provider.Name); // Ensure the name is correct
        }

        [Theory]
        [InlineData(true, 1)] // When ShowSwitchHub is true, expect 1 device
        [InlineData(false, 0)] // When ShowSwitchHub is false, expect 0 devices
        public void GetEquipment_ShouldReturnExpectedNumberOfDevices(bool showSwitchHub, int expectedCount) {
            // Arrange
            var mockProfileService = new Mock<IProfileService>();
            var provider = new DlLinkProvider(mockProfileService.Object);

            // Use reflection to set the ShowSwitchHub property
            var settingsType = typeof(Properties.Settings);
            var defaultInstance = settingsType.GetProperty("Default", BindingFlags.Static | BindingFlags.Public)?.GetValue(null);
            var showSwitchHubProperty = defaultInstance?.GetType().GetProperty("ShowSwitchHub", BindingFlags.Instance | BindingFlags.Public);
            showSwitchHubProperty?.SetValue(defaultInstance, showSwitchHub);

            // Act
            var equipment = provider.GetEquipment();

            // Assert
            Assert.NotNull(equipment);
            Assert.Equal(expectedCount, equipment.Count); // Ensure the expected number of devices is returned

            if (expectedCount > 0) {
                Assert.IsType<DlLinkDriver>(equipment[0]); // Ensure the device is a DlLinkDriver
            }
        }
    }
}