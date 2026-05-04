using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.AspNetCore.Identity;
using Moq;
using TaskFlow.Domain.Identity;
using TaskFlow.Features.Auth.Login;
using TaskFlow.Features.Auth.Register;
using TaskFlow.Infrastructure.Services;
using Xunit;

namespace TaskFlow.Api.UnitTests.Features.Auth;

public class LoginTests
{
    private readonly LoginValidator _validator;

    public LoginTests()
    {
        _validator = new LoginValidator();
    }

    [Fact]
    public void Validator_ShouldHaveError_WhenEmailIsEmpty()
    {
        var command = new LoginCommand { Email = string.Empty, Password = "password123" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validator_ShouldHaveError_WhenEmailIsInvalidFormat()
    {
        var command = new LoginCommand { Email = "invalid-email", Password = "password123" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validator_ShouldHaveError_WhenPasswordIsEmpty()
    {
        var command = new LoginCommand { Email = "user@example.com", Password = string.Empty };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Validator_ShouldPass_WhenCredentialsAreValid()
    {
        var command = new LoginCommand { Email = "user@example.com", Password = "password123" };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}

public class RegisterTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly RegisterValidator _validator;

    public RegisterTests()
    {
        var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            userStoreMock.Object,
            null!, null!, null!, null!, null!, null!, null!, null!);

        _tokenServiceMock = new Mock<ITokenService>();
        _tokenServiceMock.Setup(s => s.GenerateAccessTokenAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync("mocked-jwt-token");

        _validator = new RegisterValidator();
    }

    [Fact]
    public void Validator_ShouldHaveError_WhenEmailIsEmpty()
    {
        var command = new RegisterCommand { Email = string.Empty, Password = "TestPass123!", ConfirmPassword = "TestPass123!" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validator_ShouldHaveError_WhenEmailIsInvalidFormat()
    {
        var command = new RegisterCommand { Email = "invalid-email", Password = "TestPass123!", ConfirmPassword = "TestPass123!" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validator_ShouldHaveError_WhenPasswordIsTooShort()
    {
        var command = new RegisterCommand { Email = "user@example.com", Password = "short", ConfirmPassword = "short" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Validator_ShouldHaveError_WhenPasswordsDoNotMatch()
    {
        var command = new RegisterCommand { Email = "user@example.com", Password = "TestPass123!", ConfirmPassword = "TestPass456!" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.ConfirmPassword);
    }

    [Fact]
    public void Validator_ShouldHaveError_WhenFirstNameExceedsMaxLength()
    {
        var command = new RegisterCommand { Email = "user@example.com", Password = "TestPass123!", ConfirmPassword = "TestPass123!", FirstName = new string('A', 101) };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.FirstName);
    }

    [Fact]
    public void Validator_ShouldPass_WhenAllFieldsAreValid()
    {
        var command = new RegisterCommand { Email = "user@example.com", Password = "TestPass123!", ConfirmPassword = "TestPass123!", FirstName = "John", LastName = "Doe" };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenUserAlreadyExists()
    {
        _userManagerMock.Setup(x => x.FindByEmailAsync("user@example.com"))
            .ReturnsAsync(new ApplicationUser { Email = "user@example.com" });

        var handler = new RegisterHandler(_userManagerMock.Object, _tokenServiceMock.Object);
        var command = new RegisterCommand { Email = "user@example.com", Password = "TestPass123!", ConfirmPassword = "TestPass123!" };

        await handler.Invoking(h => h.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("User with this email already exists");
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenUserCreationFails()
    {
        _userManagerMock.Setup(x => x.FindByEmailAsync("user@example.com")).ReturnsAsync((ApplicationUser?)null);
        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), "TestPass123!"))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Unknown error" }));

        var handler = new RegisterHandler(_userManagerMock.Object, _tokenServiceMock.Object);
        var command = new RegisterCommand { Email = "user@example.com", Password = "TestPass123!", ConfirmPassword = "TestPass123!" };

        await handler.Invoking(h => h.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Registration failed: Unknown error");
    }
}
