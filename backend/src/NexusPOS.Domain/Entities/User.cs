using NexusPOS.Domain.Enums;

namespace NexusPOS.Domain.Entities;
// Identidad que puede autenticarse en NexusPOS. Almacena el hash de contraseña,
// nunca la contraseña en texto plano.
public sealed class User
{
    public int Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public UserRole Role { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Un administrador no necesita perfil Customer; un usuario Customer sí lo recibe al registrarse.
    public Customer? Customer { get; set; }
}
