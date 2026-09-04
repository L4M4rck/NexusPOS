using System.Net.Mail;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NexusPOS.Application.Auth;
using NexusPOS.Domain.Entities;
using NexusPOS.Domain.Enums;
using NexusPOS.Domain.Exceptions;
using NexusPOS.Infrastructure.Persistence;

namespace NexusPOS.Infrastructure.Auth;
// Implementa autenticación y registro utilizando EF Core, el hasher de ASP.NET
// Identity y el emisor de JWT.
internal sealed class AuthService(
    NexusPosDbContext dbContext,
    IPasswordHasher<User> passwordHasher,
    JwtTokenService tokenService) : IAuthService
{
    // Delega la emisión de un token Guest que no se persiste en MySQL.
    public AuthResponse CreateGuestToken() => tokenService.CreateGuest();

    // Normaliza el correo, valida usuario activo y verifica el hash de contraseña.
    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var email = NormalizeEmail(request.Email);
        var user = await dbContext.Users.SingleOrDefaultAsync(x => x.Email == email, cancellationToken);
        // Se usa un mensaje único para no revelar si falló el correo, la contraseña o el estado de la cuenta.
        if (user is null || !user.IsActive || passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password) == PasswordVerificationResult.Failed)
        {
            throw new BusinessException("Correo o contraseña incorrectos.", 401, "invalid-credentials");
        }

        return tokenService.CreateUser(user);
    }

    // Crea atómicamente un User con rol Customer y su perfil comercial asociado.
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        ValidateRegistration(request);
        var email = NormalizeEmail(request.Email);
        if (await dbContext.Users.AnyAsync(x => x.Email == email, cancellationToken))
        {
            throw new ConflictException("No fue posible completar el registro con los datos suministrados.", "duplicate-user");
        }

        if (await dbContext.Customers.AnyAsync(x => x.DocumentNumber == request.DocumentNumber.Trim(), cancellationToken))
        {
            throw new ConflictException("No fue posible completar el registro con los datos suministrados.", "duplicate-customer");
        }

        // EF guarda User y Customer juntos porque la navegación forma un único grafo nuevo.
        var user = new User
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = email,
            PasswordHash = string.Empty,
            Role = UserRole.Customer,
            Customer = new Customer
            {
                DocumentNumber = request.DocumentNumber.Trim(),
                Phone = request.Phone?.Trim(),
                Address = request.Address?.Trim()
            }
        };
        // Solo el hash con salt se persiste; la contraseña original nunca se guarda.
        user.PasswordHash = passwordHasher.HashPassword(user, request.Password);
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(cancellationToken);
        return tokenService.CreateUser(user);
    }

    // Consulta la identidad actual sin exponer PasswordHash ni navegaciones internas.
    public async Task<CurrentUserResponse> GetCurrentAsync(int userId, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users.AsNoTracking().SingleOrDefaultAsync(x => x.Id == userId, cancellationToken)
            ?? throw new NotFoundException("Usuario no encontrado.");
        return new CurrentUserResponse(user.Id, user.FirstName, user.LastName, user.Email, user.Role.ToString());
    }

    // Valida sintaxis y produce una representación canónica en minúsculas.
    private static string NormalizeEmail(string email)
    {
        try
        {
            return new MailAddress(email.Trim()).Address.ToLowerInvariant();
        }
        catch (FormatException)
        {
            throw new BusinessException("El correo electrónico no tiene un formato válido.");
        }
    }

    // Aplica reglas de campos obligatorios y complejidad mínima de contraseña.
    private static void ValidateRegistration(RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName))
        {
            throw new BusinessException("Nombre y apellido son obligatorios.");
        }

        if (string.IsNullOrWhiteSpace(request.DocumentNumber))
        {
            throw new BusinessException("El documento es obligatorio.");
        }

        if (request.Password.Length < 8 || !request.Password.Any(char.IsUpper) || !request.Password.Any(char.IsLower) || !request.Password.Any(char.IsDigit))
        {
            throw new BusinessException("La contraseña debe tener al menos 8 caracteres e incluir mayúscula, minúscula y número.");
        }
    }
}
