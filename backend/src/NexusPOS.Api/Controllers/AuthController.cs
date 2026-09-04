using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexusPOS.Application.Auth;

namespace NexusPOS.Api.Controllers;
// Gestiona la creación de sesiones invitadas, el inicio de sesión, el registro
// de clientes y la consulta de la identidad autenticada.
[ApiController]
[Route("api/auth")]
public sealed class AuthController(IAuthService authService) : ControllerBase
{
    // POST /api/auth/guest: emite un JWT temporal con permisos de solo lectura del catálogo.
    [AllowAnonymous]
    [HttpPost("guest")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK)]
    public ActionResult<AuthResponse> Guest() => Ok(authService.CreateGuestToken());
    // POST /api/auth/login: valida correo y contraseña y devuelve un JWT de usuario.
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken cancellationToken) =>
        Ok(await authService.LoginAsync(request, cancellationToken));
    // POST /api/auth/register: crea conjuntamente el usuario y su perfil de cliente.
    [AllowAnonymous]
    [HttpPost("register")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status201Created)]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var response = await authService.RegisterAsync(request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, response);
    }
    // GET /api/auth/me: devuelve los datos básicos del usuario identificado por el JWT.
    [Authorize(Roles = "Customer,Admin")]
    [HttpGet("me")]
    public async Task<ActionResult<CurrentUserResponse>> Me(CancellationToken cancellationToken) =>
        Ok(await authService.GetCurrentAsync(this.CurrentUserId(), cancellationToken));
}
