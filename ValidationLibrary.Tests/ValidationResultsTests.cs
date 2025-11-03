using Xunit;

namespace ValidationLibrary.Tests
{
    public class ValidationResultsTests
    {
        [Fact]
        public void ValidationResults_DefaultConstructor_SetsDefaultValues()
        {
            // Act
            var results = new ValidationResults();

            // Assert
            Assert.False(results.IsValid);
            Assert.Equal(string.Empty, results.Message);
        }

        [Theory]
        [InlineData(true, "Success")]
        [InlineData(false, "Error")]
        public void ValidationResults_Properties_CanBeSetAndRetrieved(bool isValid, string message)
        {
            // Arrange
            var results = new ValidationResults
            {
                IsValid = isValid,
                Message = message
            };

            // Assert
            Assert.Equal(isValid, results.IsValid);
            Assert.Equal(message, results.Message);
        }
    }
}