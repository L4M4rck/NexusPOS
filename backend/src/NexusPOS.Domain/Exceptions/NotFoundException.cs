namespace NexusPOS.Domain.Exceptions;

public sealed class NotFoundException(string message)
    : BusinessException(message, 404, "not-found");
