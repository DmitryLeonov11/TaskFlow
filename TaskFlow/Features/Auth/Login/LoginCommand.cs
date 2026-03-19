using MediatR;
using TaskFlow.Application.DTOs;

namespace TaskFlow.Features.Auth.Login;

public class LoginCommand : IRequest<AuthResponse>
{
    public required string Email { get; set; }
    public required string Password { get; set; }
}