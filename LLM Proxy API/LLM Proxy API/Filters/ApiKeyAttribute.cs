using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Cryptography;
using System.Text;

namespace LLM_Proxy_API.Filters
{
    /// <summary>
    /// Action filter that validates the internal X-API-KEY header against the configured key.
    /// Fails closed (returns 500) if no key is configured, and uses a constant-time comparison
    /// to avoid leaking timing information about the expected key.
    /// </summary>
    public class ApiKeyAttribute : ActionFilterAttribute
    {
        private const string HeaderName = "X-API-KEY";

        /// <summary>
        /// Validates the incoming request's API key before the action executes.
        /// </summary>
        /// <param name="context">The action executing context.</param>
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var config = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
            var expectedKey = config["InternalApiKey"];

            if (string.IsNullOrEmpty(expectedKey))
            {
                context.Result = new StatusCodeResult(StatusCodes.Status500InternalServerError);
                return;
            }

            if (!context.HttpContext.Request.Headers.TryGetValue(HeaderName, out var extractedKey) ||
                !FixedTimeEquals(extractedKey.ToString(), expectedKey))
            {
                context.Result = new UnauthorizedObjectResult("Invalid internal API key.");
                return;
            }

            base.OnActionExecuting(context);
        }

        private static bool FixedTimeEquals(string actual, string expected)
        {
            var actualBytes = Encoding.UTF8.GetBytes(actual);
            var expectedBytes = Encoding.UTF8.GetBytes(expected);

            if (actualBytes.Length != expectedBytes.Length)
            {
                // Still perform a comparison to avoid leaking length via early return timing differences.
                CryptographicOperations.FixedTimeEquals(actualBytes, actualBytes);
                return false;
            }

            return CryptographicOperations.FixedTimeEquals(actualBytes, expectedBytes);
        }
    }
}
