using MediatR;
using TaskFlow.Application.DTOs;

namespace TaskFlow.Features.Auth.Register;

public class RegisterCommand : IRequest<AuthResponse>
{
    public required string Email { get; set; }
    public required string Password { get; set; }
    public required string ConfirmPassword { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}
