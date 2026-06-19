using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace LLM_Proxy_API.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                //Attempt to process the request by passing it to the next middleware
                await _next(context);
            }
            catch (Exception ex)
            {
                //Log the actual exception for internal debugging purposes
                _logger.LogError(ex, "An error occurred in the LLM Proxy API.");

                //Send response back to the client
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/problem+json";

            //Default values for unexpected internal errors
            var problemDetails = new ProblemDetails
            {
                Status = (int)HttpStatusCode.InternalServerError,
                Title = "Internal Server Error",
                Detail = "An unexpected error occurred. Please try again later."
            };

            //Handle specific errors from the external HTTP call
            if (exception is HttpRequestException httpEx)
            {
                //If the external LLM service returns an HTTP error
                problemDetails.Status = (int?)httpEx.StatusCode ?? (int)HttpStatusCode.BadGateway;
                problemDetails.Title = "External AI Service Error";

                //Translate external errors into safe messages
                if (httpEx.StatusCode == HttpStatusCode.Unauthorized)
                {
                    problemDetails.Detail = "The system could not authenticate with the external AI service. Please contact the administrator.";
                }
                else if (httpEx.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    problemDetails.Detail = "The external AI service is currently overloaded. Please wait a moment and try again.";
                }
                else
                {
                    problemDetails.Detail = "A network error occurred while communicating with the AI service.";
                }
            }

            //Handle Timeouts
            else if (exception is TaskCanceledException)
            {
                problemDetails.Status = (int)HttpStatusCode.GatewayTimeout;
                problemDetails.Title = "Gateway Timeout";
                problemDetails.Detail = "The external AI service took too long to respond.";
            }

            //Apply the status code to the response and output the ProblemDetails as JSON
            context.Response.StatusCode = problemDetails.Status.Value;
            await context.Response.WriteAsJsonAsync(problemDetails);
        }
    }
}
