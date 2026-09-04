namespace NexusPOS.Domain.Exceptions;

// Indica que un recurso solicitado no existe o no es visible para el usuario.
public sealed class NotFoundException(string message)
    : BusinessException(message, 404, "not-found");
