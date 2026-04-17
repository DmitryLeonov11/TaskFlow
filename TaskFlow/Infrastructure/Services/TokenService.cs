using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TaskFlow.Domain.Identity;

namespace TaskFlow.Infrastructure.Services;

public class TokenService : ITokenService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly byte[] _signingKey;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _expirationMinutes;

    public TokenService(UserManager<ApplicationUser> userManager, IConfiguration configuration)
    {
        _userManager = userManager;

        var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY")
            ?? configuration["JwtSettings:Key"]
            ?? throw new InvalidOperationException("JWT Key not configured");
        _issuer = Environment.GetEnvironmentVariable("JWT_ISSUER")
            ?? configuration["JwtSettings:Issuer"]
            ?? "TaskFlow";
        _audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE")
            ?? configuration["JwtSettings:Audience"]
            ?? "FrontendApp";
        _expirationMinutes = int.TryParse(Environment.GetEnvironmentVariable("JWT_EXPIRATION_MINUTES"), out var exp)
            ? exp
            : int.Parse(configuration["JwtSettings:ExpirationMinutes"] ?? "1440");

        _signingKey = Encoding.UTF8.GetBytes(jwtKey);
    }

    public async Task<string> GenerateAccessTokenAsync(ApplicationUser user)
    {
        var key = _signingKey;

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email!),
            new("FirstName", user.FirstName ?? ""),
            new("LastName", user.LastName ?? "")
        };

        var roles = await _userManager.GetRolesAsync(user);
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_expirationMinutes),
            Issuer = _issuer,
            Audience = _audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
