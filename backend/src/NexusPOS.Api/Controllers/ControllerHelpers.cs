using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using NexusPOS.Domain.Exceptions;

namespace NexusPOS.Api.Controllers;
// Operaciones comunes para interpretar los claims del JWT desde cualquier controller.
internal static class ControllerHelpers
{
    // Obtiene el identificador numérico guardado en el claim NameIdentifier.
    public static int CurrentUserId(this ControllerBase controller)
    {
        var value = controller.User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var id) ? id : throw new BusinessException("El token no identifica un usuario válido.", 401, "invalid-token");
    }
    // Indica si la identidad autenticada pertenece al rol Admin.
    public static bool IsAdmin(this ControllerBase controller) => controller.User.IsInRole("Admin");
}
