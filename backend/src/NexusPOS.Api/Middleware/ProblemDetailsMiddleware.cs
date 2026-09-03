using Microsoft.AspNetCore.Mvc;
using NexusPOS.Domain.Exceptions;

namespace NexusPOS.Api.Middleware;

public sealed class ProblemDetailsMiddleware(RequestDelegate next, ILogger<ProblemDetailsMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (BusinessException exception)
        {
            logger.LogWarning("Business error {ErrorCode} at {Path}: {Message}", exception.ErrorCode, context.Request.Path, exception.Message);
            await WriteProblemAsync(context, exception.StatusCode, exception.ErrorCode, exception.Message);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unhandled error at {Path}", context.Request.Path);
            await WriteProblemAsync(context, 500, "internal-error", "Ocurrió un error inesperado.");
        }
    }

    private static async Task WriteProblemAsync(HttpContext context, int status, string errorCode, string detail)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.StatusCode = status;
        context.Response.ContentType = "application/problem+json";
        var problem = new ProblemDetails
        {
            Type = $"https://nexuspos/errors/{errorCode}",
            Title = errorCode.Replace('-', ' '),
            Status = status,
            Detail = detail,
            Instance = context.Request.Path
        };
        problem.Extensions["traceId"] = context.TraceIdentifier;
        await context.Response.WriteAsJsonAsync(problem);
    }
}
