using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TestTask.Core.Exceptions;

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
        if (exception is AggregateException aggregateException)
        {
            exception = aggregateException.Flatten().InnerException ?? exception;
        }


        _logger.LogError(
            exception,
            "Ошибка в запросе {Method} {Path}, User: {UserId}, TraceId: {TraceId}",
            httpContext.Request.Method,
            httpContext.Request.Path,
            httpContext.User?.Identity?.Name ?? "Anonymous",
            httpContext.TraceIdentifier);


        var statusCode = exception switch
        {
            ArgumentException => StatusCodes.Status400BadRequest,
            ValidationException => StatusCodes.Status400BadRequest,
            NotSupportedException => StatusCodes.Status400BadRequest,
            KeyNotFoundException => StatusCodes.Status404NotFound,
            ClientNotFoundException => StatusCodes.Status404NotFound,
            TransactionNotFoundException => StatusCodes.Status404NotFound,
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            InvalidOperationException => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };

        var isLocalEnvironment = httpContext.Request.Host.Value.Contains("localhost") ||
                                 httpContext.Request.Host.Value.Contains("127.0.0.1") ||
                                 httpContext.Request.Host.Value.Contains("::1");

        var detailMessage = exception.Message.Length > 500
            ? exception.Message.Substring(0, 500) + "..."
            : exception.Message;

        var detail = isLocalEnvironment
            ? detailMessage
            : "Произошла ошибка при выполнении запроса";


        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = "Произошла ошибка при выполнении запроса",
            Detail = detail,
            Type = $"https://httpstatuses.io/{statusCode}",
            Instance = httpContext.TraceIdentifier
        };


        problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;
        problemDetails.Extensions["timestamp"] = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:sszzz");
        problemDetails.Extensions["path"] = httpContext.Request.Path;

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/problem+json";

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}