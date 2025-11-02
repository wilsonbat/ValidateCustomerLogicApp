using System.Net;
using System.Net.Http.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using ValidationLibrary;
using System.Threading.Tasks;

/// <summary>
/// Azure Functions worker containing validation endpoints for email and phone.
/// Each endpoint validates the corresponding input and returns a <see cref="ValidationResults"/>.
/// </summary>
namespace Company.Function
{
    public class ValidationFunctions
    {
        private readonly ILogger _logger;
        /// <summary>
        /// Create a new instance with the provided <see cref="ILoggerFactory"/>.
        /// </summary>
        /// <param name="loggerFactory">Factory used to create an <see cref="ILogger"/> for logging request data.</param>
        public ValidationFunctions(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<ValidationFunctions>();
        }
        /// <summary>
        /// HTTP-triggered function that validates an email string.
        /// </summary>
        /// <param name="req">HTTP request data (body/query/headers available here).</param>
        /// <param name="email">Email string to validate. Expected from route or query (or null if not provided).</param>
        /// <returns>HTTP 200 response containing a <see cref="ValidationResults"/> JSON payload with IsValid and Message.</returns>
        [Function("ValidateEmail")]
        public HttpResponseData ValidateEmail(
            [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req,
            string email)
        {
            _logger.LogInformation("ValidateEmail called with: {Email}", email);

            var result = EmailValidator.Validate(email ?? "");

            var response = req.CreateResponse(HttpStatusCode.OK);
            // Note: WriteAsJsonAsync is asynchronous; using .GetAwaiter().GetResult() blocks synchronously.
            // Prefer making the function async and using await to avoid potential thread-pool blocking.
            response.WriteAsJsonAsync(result).GetAwaiter().GetResult();
            return response;
        }
        /// <summary>
        /// HTTP-triggered function that validates a US phone number string.
        /// </summary>
        /// <param name="req">HTTP request data.</param>
        /// <param name="phoneNumber">Phone number string to validate. Expected from route or query (or null if not provided).</param>
        /// <returns>HTTP 200 response containing a <see cref="ValidationResults"/> JSON payload.</returns>
        [Function("ValidatePhone")]
        public HttpResponseData ValidatePhone(
            [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req,
            string phoneNumber)
        {
            _logger.LogInformation("ValidatePhone called with: {Phone}", phoneNumber);

            var result = PhoneValidator.Validate(phoneNumber ?? "");

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.WriteAsJsonAsync(result).GetAwaiter().GetResult();
            return response;
        }

        public class ValidationRequest
        {
            public string Email { get; set; } = string.Empty;
            public string PhoneNumber { get; set; } = string.Empty;
        }
    }
}
