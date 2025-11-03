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
        public async Task<HttpResponseData> ValidateEmail(
            [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req,
            string email)
        {
            // Prefer email provided in JSON body (from callers such as Logic Apps). Fall back to route/query param.
            _logger.LogInformation("ValidateEmail called");

            ValidationRequest? body = null;
            if (req.Body != null && req.Body.CanRead)
            {
                try
                {
                    req.Body.Position = 0;
                    body = await System.Text.Json.JsonSerializer.DeserializeAsync<ValidationRequest>(req.Body);
                }
                catch
                {
                    // ignore deserialization errors and fall back to query/route value
                }
                finally
                {
                    if (req.Body.CanSeek)
                        req.Body.Position = 0;
                }
            }

            var effectiveEmail = body?.Email ?? email ?? string.Empty;
            _logger.LogInformation("ValidateEmail called with: {Email}", effectiveEmail);

            var result = EmailValidator.Validate(effectiveEmail);

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json; charset=utf-8");
            
            // Serialize with specific options to ensure clean JSON output
            var options = new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = false
            };
            var json = System.Text.Json.JsonSerializer.Serialize(result, options);
            await response.WriteStringAsync(json);
            return response;
        }
        /// <summary>
        /// HTTP-triggered function that validates a US phone number string.
        /// </summary>
        /// <param name="req">HTTP request data.</param>
        /// <param name="phoneNumber">Phone number string to validate. Expected from route or query (or null if not provided).</param>
        /// <returns>HTTP 200 response containing a <see cref="ValidationResults"/> JSON payload.</returns>
        [Function("ValidatePhone")]
        public async Task<HttpResponseData> ValidatePhone(
            [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req,
            string phoneNumber)
        {
            // Prefer phone number provided in JSON body (from callers such as Logic Apps). Fall back to route/query param.
            _logger.LogInformation("ValidatePhone called");

            ValidationRequest? body = null;
            if (req.Body != null && req.Body.CanRead)
            {
                try
                {
                    req.Body.Position = 0;
                    body = await System.Text.Json.JsonSerializer.DeserializeAsync<ValidationRequest>(req.Body);
                }
                catch
                {
                    // ignore deserialization errors and fall back to query/route value
                }
                finally
                {
                    if (req.Body.CanSeek)
                        req.Body.Position = 0;
                }
            }

            var effectivePhone = body?.PhoneNumber ?? phoneNumber ?? string.Empty;
            _logger.LogInformation("ValidatePhone called with: {Phone}", effectivePhone);

            var result = PhoneValidator.Validate(effectivePhone);

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json; charset=utf-8");
            
            // Serialize with specific options to ensure clean JSON output
            var options = new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = false
            };
            var json = System.Text.Json.JsonSerializer.Serialize(result, options);
            await response.WriteStringAsync(json);
            return response;
        }

        public class ValidationRequest
        {
            public string Email { get; set; } = string.Empty;
            public string PhoneNumber { get; set; } = string.Empty;
        }
    }
}
