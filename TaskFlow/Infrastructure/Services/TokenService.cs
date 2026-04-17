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
    private readonly IConfiguration _configuration;

    public TokenService(UserManager<ApplicationUser> userManager, IConfiguration configuration)
    {
        _userManager = userManager;
        _configuration = configuration;
    }

    public async Task<string> GenerateAccessTokenAsync(ApplicationUser user)
    {
        var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY")
            ?? _configuration["JwtSettings:Key"]
            ?? throw new InvalidOperationException("JWT Key not configured");
        var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER")
            ?? _configuration["JwtSettings:Issuer"]
            ?? "TaskFlow";
        var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE")
            ?? _configuration["JwtSettings:Audience"]
            ?? "FrontendApp";
        var jwtExpiration = int.TryParse(Environment.GetEnvironmentVariable("JWT_EXPIRATION_MINUTES"), out var exp)
            ? exp
            : int.Parse(_configuration["JwtSettings:ExpirationMinutes"] ?? "1440");

        var key = Encoding.ASCII.GetBytes(jwtKey);

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
            Expires = DateTime.UtcNow.AddMinutes(jwtExpiration),
            Issuer = jwtIssuer,
            Audience = jwtAudience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
