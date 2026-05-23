using System.Text.Json;

namespace SocialMediaPlatform.API.Middleware;

public sealed class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

    public GlobalExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            var (statusCode, title) = ex switch
            {
                ArgumentException => (StatusCodes.Status400BadRequest, "Bad request"),
                UnauthorizedAccessException => (StatusCodes.Status403Forbidden, "Forbidden"),
                InvalidOperationException => (StatusCodes.Status409Conflict, "Conflict"),
                KeyNotFoundException => (StatusCodes.Status404NotFound, "Not found"),
                _ => (StatusCodes.Status500InternalServerError, "Internal server error")
            };

            if (statusCode >= 500)
            {
                _logger.LogError(ex, "Unhandled server error for {Path}", context.Request.Path);
            }
            else
            {
                _logger.LogWarning(ex, "Handled application error for {Path}", context.Request.Path);
            }

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/problem+json";

            var response = new
            {
                type = $"https://httpstatuses.com/{statusCode}",
                title,
                status = statusCode,
                detail = ex.Message,
                traceId = context.TraceIdentifier
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}
