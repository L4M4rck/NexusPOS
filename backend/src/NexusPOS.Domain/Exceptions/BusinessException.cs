namespace NexusPOS.Domain.Exceptions;

public class BusinessException(string message, int statusCode = 400, string errorCode = "business-rule")
    : Exception(message)
{
    public int StatusCode { get; } = statusCode;
    public string ErrorCode { get; } = errorCode;
}
