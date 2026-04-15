using Microsoft.AspNetCore.Diagnostics;
using Shuryan.Application.DTOs.Common.Base;
using System.Text.Json;

namespace Shuryan.API.Middleware
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            var (statusCode, message) = MapException(exception);

            _logger.LogError(
                exception,
                "Unhandled exception. Type: {ExceptionType}, Path: {Path}, StatusCode: {StatusCode}",
                exception.GetType().Name,
                httpContext.Request.Path,
                statusCode);

            httpContext.Response.StatusCode = statusCode;
            httpContext.Response.ContentType = "application/json";

            var response = ApiResponse<object>.Failure(message, statusCode: statusCode);

            await httpContext.Response.WriteAsync(
                JsonSerializer.Serialize(response, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }),
                cancellationToken);

            return true;
        }

        private static (int StatusCode, string Message) MapException(Exception exception) =>
            exception switch
            {
                KeyNotFoundException      => (404, "Resource not found"),
                ArgumentNullException     => (400, "Invalid request"),
                ArgumentException         => (400, "Invalid request"),
                InvalidOperationException => (400, "Invalid request"),
                UnauthorizedAccessException => (403, "Access denied"),
                NotSupportedException     => (400, "Operation not supported"),
                _                         => (500, "An unexpected error occurred")
            };
    }
}
