namespace NexusPOS.Infrastructure.Auth;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";
    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = "NexusPOS";
    public string Audience { get; set; } = "NexusPOS.Client";
    public int ExpirationMinutes { get; set; } = 60;
}
