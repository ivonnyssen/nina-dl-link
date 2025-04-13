using IgorVonNyssen.NINA.DlLink.DlLinkSequenceItems;
using System.Globalization;
using System.Windows.Controls;
using Xunit;

namespace IgorVonNyssen.NINA.DlLink.Tests
{
    public class ValidationRulesTests
    {
        [Theory]
        [InlineData("1", true, null)] // Valid input: exactly 1
        [InlineData("5", true, null)] // Valid input: greater than 1
        [InlineData("0", false, "The number must be greater than or equal to 1.")] // Invalid: less than 1
        [InlineData("-1", false, "The number must be greater than or equal to 1.")] // Invalid: negative number
        [InlineData("abc", false, "Invalid integer value.")] // Invalid: non-integer
        [InlineData("", false, "Invalid integer value.")] // Invalid: empty string
        [InlineData(null, false, "Invalid integer value.")] // Invalid: null
        public void Validate_ShouldReturnExpectedResult(string input, bool expectedIsValid, string expectedErrorContent)
        {
            // Arrange
            var rule = new IntGreaterOrEqualToOne();
            var cultureInfo = CultureInfo.InvariantCulture;

            // Act
            var result = rule.Validate(input, cultureInfo);

            // Assert
            Assert.Equal(expectedIsValid, result.IsValid);
            if (!expectedIsValid)
            {
                Assert.Equal(expectedErrorContent, result.ErrorContent);
            }
        }

        [InlineData("1", true, null)] // Valid input: exactly 1
        [InlineData("5", true, null)] // Valid input: greater than 1
        [InlineData("0", false, "The number must be greater than or equal to 1.")] // Invalid: less than 1
        [InlineData("-1", false, "The number must be greater than or equal to 1.")] // Invalid: negative number
        [InlineData("1000,3", false, "Invalid integer value.")] // Invalid: non-integer
        [InlineData("", false, "Invalid integer value.")] // Invalid: empty string
        [InlineData(null, false, "Invalid integer value.")] // Invalid: null
        public void Validate_ShouldHandleCultureSpecificFormatting(string input, bool expectedIsValid, string expectedErrorContent)
        {
            // Arrange
            var rule = new IntGreaterOrEqualToOne();
            var cultureInfo = new CultureInfo("fr-FR"); // French culture uses ',' as a decimal separator

            // Act
            var result = rule.Validate(input, cultureInfo);

            // Assert
            Assert.Equal(expectedIsValid, result.IsValid);
            if (!expectedIsValid)
            {
                Assert.Equal(expectedErrorContent, result.ErrorContent);
            }
        }
    }
}