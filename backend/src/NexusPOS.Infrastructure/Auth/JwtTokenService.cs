using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NexusPOS.Application.Auth;
using NexusPOS.Domain.Entities;
using NexusPOS.Domain.Enums;

namespace NexusPOS.Infrastructure.Auth;

internal sealed class JwtTokenService(IOptions<JwtOptions> options)
{
    private readonly JwtOptions _options = options.Value;

    public AuthResponse CreateGuest()
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(Math.Min(_options.ExpirationMinutes, 30));
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, $"guest:{Guid.NewGuid():N}"),
            new Claim(ClaimTypes.Role, UserRole.Guest.ToString())
        };
        return new AuthResponse(CreateToken(claims, expiresAt), expiresAt, UserRole.Guest.ToString(), null);
    }

    public AuthResponse CreateUser(User user)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(_options.ExpirationMinutes);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };
        return new AuthResponse(CreateToken(claims, expiresAt), expiresAt, user.Role.ToString(), $"{user.FirstName} {user.LastName}");
    }

    private string CreateToken(IEnumerable<Claim> claims, DateTime expiresAt)
    {
        if (_options.Secret.Length < 32)
        {
            throw new InvalidOperationException("JWT_SECRET debe contener al menos 32 caracteres.");
        }

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Secret)),
            SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(_options.Issuer, _options.Audience, claims, expires: expiresAt, signingCredentials: credentials);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
