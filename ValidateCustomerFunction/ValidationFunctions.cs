using System.Net;
using System.Net.Http.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using ValidationLibrary;
using System.Threading.Tasks;

namespace Company.Function
{
    public class ValidationFunctions
    {
        private readonly ILogger _logger;

        public ValidationFunctions(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<ValidationFunctions>();
        }

        [Function("ValidateEmail")]
        public HttpResponseData ValidateEmail(
            [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req,
            string email)
        {
            _logger.LogInformation("ValidateEmail called with: {Email}", email);

            var result = EmailValidator.Validate(email ?? "");

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.WriteAsJsonAsync(result).GetAwaiter().GetResult();
            return response;
        }

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
