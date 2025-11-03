using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using Microsoft.Extensions.Logging;
using Moq;
using ValidationLibrary;

namespace Company.Function.Tests
{
    public class ValidationFunctionsTests
    {
        private readonly Mock<ILogger<ValidationFunctions>> _loggerMock;
        private readonly Mock<ILoggerFactory> _loggerFactoryMock;
        private readonly ValidationFunctions _functions;

        public ValidationFunctionsTests()
        {
            // Setup logger mocks
            _loggerMock = new Mock<ILogger<ValidationFunctions>>();
            _loggerFactoryMock = new Mock<ILoggerFactory>();
            _loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>()))
                            .Returns(_loggerMock.Object);

            // Create the functions instance with mocked logger
            _functions = new ValidationFunctions(_loggerFactoryMock.Object);
        }

        private (Mock<HttpRequestData> request, Mock<HttpResponseData> response) SetupHttpMocks(FunctionContext? context = null)
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.Configure<JsonSerializerOptions>(options => { });
            var services = serviceCollection.BuildServiceProvider();

            var contextMock = new Mock<FunctionContext>();
            contextMock.Setup(c => c.InstanceServices).Returns(services);
            context = contextMock.Object;

            var request = new Mock<HttpRequestData>(context);
            var response = new Mock<HttpResponseData>(context);

            var responseStream = new MemoryStream();
            response.Setup(r => r.Body).Returns(responseStream);
            response.Setup(r => r.Headers).Returns(new HttpHeadersCollection());
            response.SetupProperty(r => r.StatusCode);

            request.Setup(r => r.CreateResponse())
                .Returns(response.Object);

            return (request, response);
        }

        [Theory]
        [InlineData("test@example.com", true, "Email is valid.")]
        [InlineData("", false, "Email is required.")]
        [InlineData(null, false, "Email is required.")]
        [InlineData("invalid-email", false, "Invalid email format.")]
        public async Task ValidateEmail_ReturnsExpectedResponse(string email, bool expectedIsValid, string expectedMessage)
        {
            // Arrange
            var (request, response) = SetupHttpMocks();
            // Simulate Logic App sending JSON payload in the request body
            var requestObj = new ValidationFunctions.ValidationRequest { Email = email };
            var requestJson = JsonSerializer.Serialize(requestObj);
            var utf8NoBom = new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
            var requestStream = new MemoryStream(utf8NoBom.GetBytes(requestJson ?? string.Empty));
            requestStream.Position = 0;
            request.Setup(r => r.Body).Returns(requestStream);

            // Act
            var result = await _functions.ValidateEmail(request.Object, email);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            // verify an informational log was written (content varies depending on input)
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);

            var responseContent = GetResponseContent(response);
            Assert.Equal(expectedIsValid, responseContent.IsValid);
            Assert.Equal(expectedMessage, responseContent.Message);
        }

        [Theory]
        [InlineData("1234567890", true, "Phone number is valid.")]
        [InlineData("", false, "Phone number is required.")]
        [InlineData(null, false, "Phone number is required.")]
        [InlineData("123-456-7890", true, "Phone number is valid.")]
        [InlineData("invalid", false, "Invalid phone number format.")]
        [InlineData("123", false, "Phone number must be 10 digits.")]
        public async Task ValidatePhone_ReturnsExpectedResponse(string phoneNumber, bool expectedIsValid, string expectedMessage)
        {
            // Arrange
            var (request, response) = SetupHttpMocks();
            // Simulate Logic App sending JSON payload in the request body
            var requestObj = new ValidationFunctions.ValidationRequest { PhoneNumber = phoneNumber };
            var requestJson = JsonSerializer.Serialize(requestObj);
            var utf8NoBom = new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
            var requestStream = new MemoryStream(utf8NoBom.GetBytes(requestJson ?? string.Empty));
            requestStream.Position = 0;
            request.Setup(r => r.Body).Returns(requestStream);

            // Act
            var result = await _functions.ValidatePhone(request.Object, phoneNumber);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            // verify an informational log was written (content varies depending on input)
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);

            var responseContent = GetResponseContent(response);
            Assert.Equal(expectedIsValid, responseContent.IsValid);
            Assert.Equal(expectedMessage, responseContent.Message);
        }

        private ValidationResults GetResponseContent(Mock<HttpResponseData> responseMock)
        {
            responseMock.Object.Body.Position = 0;
            using var reader = new StreamReader(responseMock.Object.Body);
            var json = reader.ReadToEnd();
            return JsonSerializer.Deserialize<ValidationResults>(json)!;
        }

        [Fact]
        public void ValidationRequest_DefaultConstructor_SetsEmptyStrings()
        {
            // Arrange & Act
            var request = new ValidationFunctions.ValidationRequest();

            // Assert
            Assert.Equal(string.Empty, request.Email);
            Assert.Equal(string.Empty, request.PhoneNumber);
        }
    }
}