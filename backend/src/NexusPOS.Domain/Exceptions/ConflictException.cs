namespace NexusPOS.Domain.Exceptions;

public sealed class ConflictException(string message, string errorCode = "conflict")
    : BusinessException(message, 409, errorCode);
