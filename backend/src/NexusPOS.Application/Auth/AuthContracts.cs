namespace NexusPOS.Application.Auth;

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

public interface IAuthService
{
    AuthResponse CreateGuestToken();
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
    Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken);
    Task<CurrentUserResponse> GetCurrentAsync(int userId, CancellationToken cancellationToken);
}
