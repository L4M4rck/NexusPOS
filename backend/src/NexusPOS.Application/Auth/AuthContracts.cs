namespace NexusPOS.Application.Auth;

// Objetos de entrada y salida de autenticación. Password solo entra; PasswordHash nunca sale de la API.
public sealed record LoginRequest(string Email, string Password);

public sealed record RegisterRequest(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string DocumentNumber,
    string? Phone,
    string? Address);

public sealed record AuthResponse(
    string AccessToken,
    DateTime ExpiresAt,
    string Role,
    string? DisplayName);

public sealed record CurrentUserResponse(
    int Id,
    string FirstName,
    string LastName,
    string Email,
    string Role);
// Define los casos de uso de autenticación disponibles para la capa API.
public interface IAuthService
{
    // Emite una identidad temporal Guest sin crear registros en la base.
    AuthResponse CreateGuestToken();

    // Valida credenciales y emite un token para un usuario activo.
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken);

    // Crea un usuario Customer, su perfil comercial y su primer JWT.
    Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken);

    // Proyecta los datos públicos del usuario autenticado.
    Task<CurrentUserResponse> GetCurrentAsync(int userId, CancellationToken cancellationToken);
}
