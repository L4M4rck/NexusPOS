namespace NexusPOS.Domain.Exceptions;
// Error esperado de negocio con estado HTTP y código semántico. El middleware
// lo transforma en Problem Details sin tratarlo como un fallo inesperado.
public class BusinessException(string message, int statusCode = 400, string errorCode = "business-rule")
    : Exception(message)
{
    public int StatusCode { get; } = statusCode;
    public string ErrorCode { get; } = errorCode;
}
