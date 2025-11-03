using Xunit;
using ValidationLibrary;

namespace ValidationLibrary.Tests
{
    public class PhoneValidatorTests
    {
        [Theory]
        [InlineData("1234567890", true, "Phone number is valid.")]
        [InlineData("123-456-7890", true, "Phone number is valid.")]
        [InlineData("(123) 456-7890", true, "Phone number is valid.")]
        [InlineData("123.456.7890", true, "Phone number is valid.")]
        [InlineData("+1 123-456-7890", true, "Phone number is valid.")]
        [InlineData("1-123-456-7890", true, "Phone number is valid.")]
        [InlineData("", false, "Phone number is required.")]
        [InlineData(" ", false, "Phone number is required.")]
        [InlineData("123abc4567", false, "Invalid phone number format.")]
        [InlineData("abc", false, "Invalid phone number format.")]
        [InlineData("123456", false, "Phone number must be 10 digits.")]
        [InlineData("22345678901", false, "Phone number must be 10 digits.")]
        public void Validate_WithVariousInputs_ReturnsExpectedResults(string phoneNumber, bool expectedIsValid, string expectedMessage)
        {
            // Act
            var result = PhoneValidator.Validate(phoneNumber);

            // Assert
            Assert.Equal(expectedIsValid, result.IsValid);
            Assert.Equal(expectedMessage, result.Message);
        }

        [Theory]
        [InlineData("(123) 456-7890")]
        [InlineData("123-456-7890")]
        [InlineData("123.456.7890")]
        [InlineData("+1 123-456-7890")]
        public void Validate_WithDifferentFormats_AcceptsValidFormats(string phoneNumber)
        {
            // Act
            var result = PhoneValidator.Validate(phoneNumber);

            // Assert
            Assert.True(result.IsValid);
            Assert.Equal("Phone number is valid.", result.Message);
        }
    }
}