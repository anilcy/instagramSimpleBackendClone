/*
using System.Text;
using System.Text.Json;
using FluentAssertions;
using SocialMediaPlatform.API.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;

namespace SocialMediaPlatform.Tests.Tests.Middleware;

public class GlobalExceptionHandlingMiddlewareTests
{
    public static IEnumerable<object[]> ExceptionCases => new[]
    {
        new object[] { new ArgumentException("bad input"), StatusCodes.Status400BadRequest, "Bad request" },
        new object[] { new UnauthorizedAccessException("no access"), StatusCodes.Status403Forbidden, "Forbidden" },
        new object[] { new InvalidOperationException("conflict happened"), StatusCodes.Status409Conflict, "Conflict" },
        new object[] { new KeyNotFoundException("missing item"), StatusCodes.Status404NotFound, "Not found" },
        new object[] { new Exception("unexpected boom"), StatusCodes.Status500InternalServerError, "Internal server error" }
    };

    [Theory]
    [MemberData(nameof(ExceptionCases))]
    public async Task InvokeAsync_ShouldMapExceptionsToProblemDetails(Exception exception, int expectedStatusCode, string expectedTitle)
    {
        var logger = new Mock<ILogger<GlobalExceptionHandlingMiddleware>>();
        RequestDelegate next = _ => throw exception;
        var middleware = new GlobalExceptionHandlingMiddleware(next, logger.Object);
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/test";
        context.TraceIdentifier = "trace-id-1";
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(expectedStatusCode);
        context.Response.ContentType.Should().Be("application/problem+json");

        context.Response.Body.Position = 0;
        using var reader = new StreamReader(context.Response.Body, Encoding.UTF8, leaveOpen: true);
        var payload = await reader.ReadToEndAsync();
        var document = JsonDocument.Parse(payload);

        document.RootElement.GetProperty("title").GetString().Should().Be(expectedTitle);
        document.RootElement.GetProperty("status").GetInt32().Should().Be(expectedStatusCode);
        document.RootElement.GetProperty("detail").GetString().Should().Be(exception.Message);
        document.RootElement.GetProperty("traceId").GetString().Should().Be("trace-id-1");
    }
}
*/

