using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CustomerManagement.Contracts.Auth;
using Microsoft.IdentityModel.Tokens;

namespace CustomerManagement.Api.Services;

public class AuthService : IAuthService
{
    private readonly string _adminUsername;
    private readonly string _adminPassword;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly string _secretKey;
    private readonly int _expiryMinutes;

    public AuthService(IConfiguration configuration)
    {
        _adminUsername = GetRequired(configuration, "AdminAccount:Username");
        _adminPassword = GetRequired(configuration, "AdminAccount:Password");
        _issuer = GetRequired(configuration, "Jwt:Issuer");
        _audience = GetRequired(configuration, "Jwt:Audience");
        _secretKey = GetRequired(configuration, "Jwt:SecretKey");

        if (Encoding.UTF8.GetByteCount(_secretKey) < 32)
        {
            throw new InvalidOperationException(
                "Jwt:SecretKey phải có ít nhất 32 byte.");
        }

        if (!int.TryParse(configuration["Jwt:ExpiryMinutes"], out var minutes)
            || minutes <= 0)
        {
            throw new InvalidOperationException(
                "Jwt:ExpiryMinutes phải là số nguyên dương.");
        }

        _expiryMinutes = minutes;
    }

    public LoginResponse? Login(LoginRequest request)
    {
        var usernameMatches = string.Equals(
            request.Username.Trim(),
            _adminUsername,
            StringComparison.Ordinal);

        var inputHash = SHA256.HashData(
            Encoding.UTF8.GetBytes(request.Password));

        var expectedHash = SHA256.HashData(
            Encoding.UTF8.GetBytes(_adminPassword));

        var passwordMatches = CryptographicOperations.FixedTimeEquals(
            inputHash,
            expectedHash);

        if (!usernameMatches || !passwordMatches)
        {
            return null;
        }

        var now = DateTimeOffset.UtcNow;
        var expiresAt = now.AddMinutes(_expiryMinutes);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, _adminUsername),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("name", _adminUsername),
            new Claim("role", "Admin")
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_secretKey));

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            notBefore: now.UtcDateTime,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials);

        return new LoginResponse
        {
            AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
            ExpiresAtUtc = expiresAt,
            Username = _adminUsername,
            Role = "Admin"
        };
    }

    private static string GetRequired(
        IConfiguration configuration,
        string key)
    {
        var value = configuration[key];

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(
                $"Thiếu cấu hình {key}.");
        }

        return value;
    }
}