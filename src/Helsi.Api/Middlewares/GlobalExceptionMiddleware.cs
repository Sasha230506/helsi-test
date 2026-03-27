using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace Helsi.Api.Middlewares;

public sealed class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception on {Method} {Path}",
                httpContext.Request.Method, httpContext.Request.Path);
            await WriteProblemAsync(httpContext, ex);
        }
    }

    private static Task WriteProblemAsync(HttpContext ctx, Exception ex)
    {
        var (status, title) = MapException(ex);

        var problem = new ProblemDetails
        {
            Type = ex.GetType().Name,
            Title = title,
            Detail = ex.Message,
            Status = (int)status,
            Instance = ctx.Request.Path,
        };

        var json = JsonSerializer.Serialize(problem, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        });

        ctx.Response.ContentType = "application/problem+json";
        ctx.Response.StatusCode = (int)status;
        return ctx.Response.WriteAsync(json);
    }

    private static (HttpStatusCode status, string title) MapException(Exception ex) =>
        ex switch
        {
            ArgumentException => (HttpStatusCode.BadRequest, "Validation error"),
            UnauthorizedAccessException => (HttpStatusCode.Forbidden, "Access denied"),
            KeyNotFoundException => (HttpStatusCode.NotFound, "Not found"),
            InvalidOperationException => (HttpStatusCode.Conflict, "Operation conflict"),
            _ => (HttpStatusCode.InternalServerError, "Unexpected error")
        };
}