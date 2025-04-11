using Xunit;
using Moq;
using System.Net.Http;
using IgorVonNyssen.NINA.DlLink.DlLinkSequenceItems;
using IgorVonNyssen.NINA.DlLink.DlLinkDrivers;
using NINA.Core.Model;
using NINA.Sequencer.SequenceItem;
using System.Threading.Tasks;
using System;

namespace IgorVonNyssen.NINA.DlLink.Tests {

    public class DlLinkConditionTests {
        private readonly Mock<HttpClient> mockHttpClient = new Mock<HttpClient>();

        [Fact]
        public void Check_ShouldReturnTrue_WhenOutletStateMatchesExpectedState_On() {
            // Arrange
            var condition = new DlLinkCondition {
                HttpClient = mockHttpClient.Object,
                ServerAddress = "http://localhost",
                UserName = "user",
                Password = "password",
                OutletNumber = 1,
                State = OutletStates.On
            };

            // Mock HttpUtils.GetOutletState to return true
            var mockResult = Result<bool>.Ok(true);
            Mock.Get(HttpUtils.GetOutletState)
                .Setup(m => m(It.IsAny<HttpClient>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockResult);

            // Act
            var result = condition.Check(null, null);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Check_ShouldReturnFalse_WhenOutletStateDoesNotMatchExpectedState_Off() {
            // Arrange
            var condition = new DlLinkCondition {
                HttpClient = mockHttpClient.Object,
                ServerAddress = "http://localhost",
                UserName = "user",
                Password = "password",
                OutletNumber = 1,
                State = OutletStates.Off
            };

            // Mock HttpUtils.GetOutletState to return true
            var mockResult = Result<bool>.Ok(true);
            Mock.Get(HttpUtils.GetOutletState)
                .Setup(m => m(It.IsAny<HttpClient>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockResult);

            // Act
            var result = condition.Check(null, null);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Check_ShouldThrowException_WhenHttpUtilsFails() {
            // Arrange
            var condition = new DlLinkCondition {
                HttpClient = mockHttpClient.Object,
                ServerAddress = "http://localhost",
                UserName = "user",
                Password = "password",
                OutletNumber = 1,
                State = OutletStates.On
            };

            // Mock HttpUtils.GetOutletState to return an error
            var mockResult = Result<bool>.Err();
            Mock.Get(HttpUtils.GetOutletState)
                .Setup(m => m(It.IsAny<HttpClient>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockResult);

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