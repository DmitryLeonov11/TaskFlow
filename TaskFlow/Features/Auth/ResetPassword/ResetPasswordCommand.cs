using MediatR;

namespace TaskFlow.Features.Auth.ResetPassword;

public class RequestPasswordResetCommand : IRequest
{
    public required string Email { get; set; }
}

public class ResetPasswordCommand : IRequest
{
    public required string Email { get; set; }
    public required string Token { get; set; }
    public required string NewPassword { get; set; }
}

