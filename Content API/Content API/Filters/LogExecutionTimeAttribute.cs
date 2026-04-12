using Microsoft.AspNetCore.Mvc.Filters;
using System.Diagnostics;

namespace Content_API.Filters
{
    /// <summary>
    /// This filter measures the execution time of an action.
    /// </summary>
    public class LogExecutionTimeAttribute : ActionFilterAttribute
    {
        private Stopwatch? _stopwatch;

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            _stopwatch = Stopwatch.StartNew();
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            _stopwatch?.Stop();
            var elapsed = _stopwatch?.ElapsedMilliseconds;

            // Execution time is logged to the Output console in Visual Studio
            Console.WriteLine($"[Timer] {context.ActionDescriptor.DisplayName} took {elapsed}ms");
        }
    }
}
