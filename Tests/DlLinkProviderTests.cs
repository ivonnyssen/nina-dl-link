using IgorVonNyssen.NINA.DlLink.DlLinkDrivers;
using Moq;
using NINA.Profile.Interfaces;
using Xunit;
using System.Collections.Generic;

namespace IgorVonNyssen.NINA.DlLink.Tests
{
    public class DlLinkProviderTests
    {
        [Fact]
        public void GetEquipment_ShouldReturnDlLinkDriver()
        {
            // Arrange
            var mockProfileService = new Mock<IProfileService>();
            var provider = new DlLinkProvider(mockProfileService.Object);

            // Act
            var equipment = provider.GetEquipment();

            // Assert
            Assert.NotNull(equipment);
            Assert.Single(equipment); // Ensure only one device is returned
            Assert.IsType<DlLinkDriver>(equipment[0]); // Ensure the device is a DlLinkDriver
        }
    }
}