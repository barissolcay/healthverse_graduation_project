using HealthVerse.SharedKernel.Domain;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace HealthVerse.Api.Infrastructure;

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
        // Log all exceptions
        _logger.LogError(exception, "Exception occurred: {Message}", exception.Message);

        var (statusCode, problemDetails) = exception switch
        {
            // Domain exceptions → 400 Bad Request
            DomainException domainEx => (
                StatusCodes.Status400BadRequest,
                CreateProblemDetails(
                    StatusCodes.Status400BadRequest,
                    "Domain Error",
                    domainEx.Message,
                    httpContext.Request.Path,
                    new Dictionary<string, object?> { ["code"] = domainEx.Code }
                )
            ),

            // Unauthorized → 401
            UnauthorizedAccessException => (
                StatusCodes.Status401Unauthorized,
                CreateProblemDetails(
                    StatusCodes.Status401Unauthorized,
                    "Unauthorized",
                    "Kimlik doğrulama gerekli.",
                    httpContext.Request.Path
                )
            ),

            // Argument exceptions → 400
            ArgumentException argEx => (
                StatusCodes.Status400BadRequest,
                CreateProblemDetails(
                    StatusCodes.Status400BadRequest,
                    "Invalid Argument",
                    argEx.Message,
                    httpContext.Request.Path
                )
            ),

            // Everything else → 500
            _ => (
                StatusCodes.Status500InternalServerError,
                CreateProblemDetails(
                    StatusCodes.Status500InternalServerError,
                    "Internal Server Error",
                    "An unexpected error occurred. Please try again later.",
                    httpContext.Request.Path
                )
            )
        };

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/problem+json";
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    private static ProblemDetails CreateProblemDetails(
        int status,
        string title,
        string detail,
        string instance,
        Dictionary<string, object?>? extensions = null)
    {
        var problemDetails = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail,
            Instance = instance
        };

        if (extensions != null)
        {
            foreach (var (key, value) in extensions)
            {
                problemDetails.Extensions[key] = value;
            }
        }

        return problemDetails;
    }
}
