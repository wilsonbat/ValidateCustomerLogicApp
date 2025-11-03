using Xunit;
using ValidationLibrary;

namespace ValidationLibrary.Tests
{
    public class EmailValidatorTests
    {
        [Theory]
        [InlineData("test@example.com", true, "Email is valid.")]
        [InlineData("user.name@domain.com", true, "Email is valid.")]
        [InlineData("user+tag@example.com", true, "Email is valid.")]
        [InlineData("TEST@EXAMPLE.COM", true, "Email is valid.")]
        [InlineData("", false, "Email is required.")]
        [InlineData(" ", false, "Email is required.")]
        [InlineData("invalid", false, "Invalid email format.")]
        [InlineData("invalid@", false, "Invalid email format.")]
        [InlineData("@invalid.com", false, "Invalid email format.")]
        [InlineData("invalid@domain", false, "Invalid email format.")]
        [InlineData("invalid@.com", false, "Invalid email format.")]
        [InlineData("@", false, "Invalid email format.")]
        public void Validate_WithVariousInputs_ReturnsExpectedResults(string email, bool expectedIsValid, string expectedMessage)
        {
            // Act
            var result = EmailValidator.Validate(email);

            // Assert
            Assert.Equal(expectedIsValid, result.IsValid);
            Assert.Equal(expectedMessage, result.Message);
        }
    }
}