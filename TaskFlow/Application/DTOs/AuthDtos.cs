namespace TaskFlow.Application.DTOs;

public record LoginRequest(string Email, string Password);

public record RegisterRequest(
    string Email,
    string Password,
    string ConfirmPassword,
    string? FirstName,
    string? LastName);

public record AuthResponse(
    string Id,
    string Email,
    string? FirstName,
    string? LastName,
    string AccessToken,
    string RefreshToken);

public record RefreshTokenRequest(string RefreshToken);

public record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword,
    string ConfirmPassword);