using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using TaskFlow.Application.DTOs;
using TaskFlow.Domain.Identity;
using TaskFlow.Infrastructure.Services;

namespace TaskFlow.Features.Auth.Login;

public class LoginHandler : IRequestHandler<LoginCommand, AuthResponse>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ITokenService _tokenService;

    public LoginHandler(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ITokenService tokenService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
    }

    public async Task<AuthResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            throw new SecurityTokenException("Invalid email or password");
        }

        if (!user.IsActive)
        {
            throw new InvalidOperationException("User account is deactivated");
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            if (result.IsLockedOut)
            {
                throw new InvalidOperationException("Account is locked out. Please try again later.");
            }
            throw new SecurityTokenException("Invalid email or password");
        }

        user.LastLoginAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        var accessToken = await _tokenService.GenerateAccessTokenAsync(user);
        var refreshToken = Guid.NewGuid().ToString();

        // TODO: Store refresh token in database

        return new AuthResponse(
            user.Id,
            user.Email!,
            user.FirstName,
            user.LastName,
            accessToken,
            refreshToken
        );
    }
}
