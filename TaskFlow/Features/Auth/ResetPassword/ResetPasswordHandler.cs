using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using TaskFlow.Domain.Identity;

namespace TaskFlow.Features.Auth.ResetPassword;

public class RequestPasswordResetHandler : IRequestHandler<RequestPasswordResetCommand>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<RequestPasswordResetHandler> _logger;

    public RequestPasswordResetHandler(
        UserManager<ApplicationUser> userManager,
        ILogger<RequestPasswordResetHandler> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async Task Handle(RequestPasswordResetCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            // Не раскрываем, существует ли пользователь
            _logger.LogInformation("Password reset requested for non-existing email {Email}", request.Email);
            return;
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        // TODO: интеграция с email‑рассылкой. Пока логируем токен.
        _logger.LogInformation("Password reset token for {Email}: {Token}", request.Email, token);
    }
}

public class ResetPasswordHandler : IRequestHandler<ResetPasswordCommand>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public ResetPasswordHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to reset password: {errors}");
        }
    }
}