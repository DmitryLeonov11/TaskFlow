using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TaskFlow.Application.DTOs;
using TaskFlow.Features.Auth.Login;
using TaskFlow.Features.Auth.Register;
using Xunit;

namespace TaskFlow.Api.IntegrationTests.Features.Auth;

public class AuthEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthEndpointsTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_ShouldReturnSuccess_WhenDataIsValid()
    {
        // Arrange
        var command = new RegisterCommand
        {
            Email = $"test_{Guid.NewGuid()}@example.com",
            Password = "TestPass123!",
            ConfirmPassword = "TestPass123!",
            FirstName = "Test",
            LastName = "User"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
        result.Should().NotBeNull();
        result!.Email.Should().Be(command.Email);
        result.FirstName.Should().Be("Test");
        result.LastName.Should().Be("User");
        result.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Register_ShouldReturnBadRequest_WhenEmailAlreadyExists()
    {
        // Arrange
        var email = $"duplicate_{Guid.NewGuid()}@example.com";
        var registerCommand = new RegisterCommand
        {
            Email = email,
            Password = "TestPass123!",
            ConfirmPassword = "TestPass123!",
            FirstName = "Test",
            LastName = "User"
        };

        // Register first user
        await _client.PostAsJsonAsync("/api/auth/register", registerCommand);

        // Try to register again with same email
        var duplicateCommand = new RegisterCommand
        {
            Email = email,
            Password = "TestPass123!",
            ConfirmPassword = "TestPass123!",
            FirstName = "Test2",
            LastName = "User2"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", duplicateCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_ShouldReturnBadRequest_WhenValidationFails()
    {
        // Arrange
        var command = new RegisterCommand
        {
            Email = "invalid-email",
            Password = "short",
            ConfirmPassword = "different"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_ShouldReturnSuccess_WhenCredentialsAreValid()
    {
        // Arrange - Register a user first
        var email = $"login_test_{Guid.NewGuid()}@example.com";
        var password = "TestPass123!";
        
        var registerCommand = new RegisterCommand
        {
            Email = email,
            Password = password,
            ConfirmPassword = password,
            FirstName = "Login",
            LastName = "Test"
        };

        await _client.PostAsJsonAsync("/api/auth/register", registerCommand);

        // Act - Login
        var loginCommand = new LoginCommand
        {
            Email = email,
            Password = password
        };

        var response = await _client.PostAsJsonAsync("/api/auth/login", loginCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
        result.Should().NotBeNull();
        result!.Email.Should().Be(email);
        result.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorized_WhenCredentialsAreInvalid()
    {
        // Arrange
        var command = new LoginCommand
        {
            Email = "nonexistent@example.com",
            Password = "WrongPassword123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_ShouldReturnBadRequest_WhenValidationFails()
    {
        // Arrange
        var command = new LoginCommand
        {
            Email = "",
            Password = ""
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorized_WhenPasswordIsWrong()
    {
        // Arrange - Register a user first
        var email = $"wrong_pass_{Guid.NewGuid()}@example.com";
        var password = "TestPass123!";
        
        var registerCommand = new RegisterCommand
        {
            Email = email,
            Password = password,
            ConfirmPassword = password,
            FirstName = "Test",
            LastName = "User"
        };

        await _client.PostAsJsonAsync("/api/auth/register", registerCommand);

        // Act - Login with wrong password
        var loginCommand = new LoginCommand
        {
            Email = email,
            Password = "WrongPassword456!"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/login", loginCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
