using Microsoft.AspNetCore.Mvc;
using NexusPOS.Domain.Exceptions;

namespace NexusPOS.Api.Middleware;
// Centraliza el tratamiento de errores y garantiza respuestas Problem Details
// uniformes sin exponer detalles internos al cliente HTTP.
public sealed class ProblemDetailsMiddleware(RequestDelegate next, ILogger<ProblemDetailsMiddleware> logger)
{
    // Ejecuta el siguiente componente y transforma cualquier excepción no controlada.
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (BusinessException exception)
        {
            // Las excepciones de negocio conservan el estado HTTP y el código semántico definidos en Domain.
            logger.LogWarning("Business error {ErrorCode} at {Path}: {Message}", exception.ErrorCode, context.Request.Path, exception.Message);
            await WriteProblemAsync(context, exception.StatusCode, exception.ErrorCode, exception.Message);
        }
        catch (Exception exception)
        {
            // El detalle técnico queda en logs; al consumidor se entrega un mensaje seguro y genérico.
            logger.LogError(exception, "Unhandled error at {Path}", context.Request.Path);
            await WriteProblemAsync(context, 500, "internal-error", "Ocurrió un error inesperado.");
        }
    }
    // Escribe el formato RFC 7807 e incluye traceId para correlacionar respuesta y logs.
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
