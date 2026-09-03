using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using NexusPOS.Domain.Exceptions;

namespace NexusPOS.Api.Controllers;

internal static class ControllerHelpers
{
    public static int CurrentUserId(this ControllerBase controller)
    {
        var value = controller.User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var id) ? id : throw new BusinessException("El token no identifica un usuario válido.", 401, "invalid-token");
    }

    public static bool IsAdmin(this ControllerBase controller) => controller.User.IsInRole("Admin");
}
