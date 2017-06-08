using System;
using System.Threading.Tasks;
using Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace RestAPI.Helpers
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;

        public LoggingMiddleware(RequestDelegate next, ILogger logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            var cameIn = DateTime.Now;
            await _next(context);
            await _logger.Information(Guid.Empty, $"{context.Request.Method} {context.Request.Path} " +
                                                  $"handled in {(DateTime.Now - cameIn).Milliseconds}ms " +
                                                  $"with response {context.Response.StatusCode}");
        }
    }

    public static class LoggingMiddlewareHelper
    {
        public static void UseLoggingMiddleware(this IApplicationBuilder app)
        {
            app.UseMiddleware<LoggingMiddleware>();
        }
    }
}