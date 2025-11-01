using System.Net;
using System.Net.Http.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using ValidationLibrary;
using System.Threading.Tasks;  // <--- important for async/await

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
        public async Task<HttpResponseData> ValidateEmail(
            [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
        {
            var request = await req.ReadFromJsonAsync<ValidationRequest>();
            var email = request?.Email ?? string.Empty;
            var result = EmailValidator.Validate(email);

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(result);
            return response;
        }

        [Function("ValidatePhone")]
        public async Task<HttpResponseData> ValidatePhone(
            [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
        {
            var request = await req.ReadFromJsonAsync<ValidationRequest>();
            var phone = request?.PhoneNumber ?? string.Empty;
            var result = PhoneValidator.Validate(phone);

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(result);
            return response;
        }

        public class ValidationRequest
        {
            public string Email { get; set; } = string.Empty;
            public string PhoneNumber { get; set; } = string.Empty;
        }
    }
}
