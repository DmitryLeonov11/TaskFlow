using TaskFlow.Domain.Identity;

namespace TaskFlow.Infrastructure.Services;

public interface ITokenService
{
    Task<string> GenerateAccessTokenAsync(ApplicationUser user);
}
