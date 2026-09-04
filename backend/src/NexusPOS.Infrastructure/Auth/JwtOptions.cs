namespace NexusPOS.Infrastructure.Auth;

// Configuración enlazada desde la sección Jwt de appsettings o variables de entorno.
public sealed class JwtOptions
{
    // Nombre de la sección de configuración.
    public const string SectionName = "Jwt";

    // Clave privada utilizada para firmar; debe tener al menos 32 caracteres.
    public string Secret { get; set; } = string.Empty;

    // Identifica al sistema que emite el token.
    public string Issuer { get; set; } = "NexusPOS";

    // Identifica al cliente para el cual es válido el token.
    public string Audience { get; set; } = "NexusPOS.Client";

    // Duración de las sesiones autenticadas.
    public int ExpirationMinutes { get; set; } = 60;
}
