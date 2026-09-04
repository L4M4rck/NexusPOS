namespace NexusPOS.Domain.Exceptions;

// Representa conflictos 409, por ejemplo stock insuficiente o pago rechazado.
public sealed class ConflictException(string message, string errorCode = "conflict")
    : BusinessException(message, 409, errorCode);
