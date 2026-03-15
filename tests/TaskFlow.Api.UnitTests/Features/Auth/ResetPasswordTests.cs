using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using TaskFlow.Domain.Identity;
using TaskFlow.Features.Auth.ResetPassword;
using Xunit;

namespace TaskFlow.Api.UnitTests.Features.Auth;

public class ResetPasswordTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<ILogger<RequestPasswordResetHandler>> _loggerMock;

    public ResetPasswordTests()
    {
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            Mock.Of<IUserStore<ApplicationUser>>(),
            null!, null!, null!, null!, null!, null!, null!, null!);

        _loggerMock = new Mock<ILogger<RequestPasswordResetHandler>>();
    }

    [Fact]
    public async Task RequestPasswordReset_ShouldGenerateToken_WhenUserExists()
    {
        // Arrange
        var user = new ApplicationUser { Email = "user@example.com", UserName = "user@example.com" };

        _userManagerMock.Setup(x => x.FindByEmailAsync(user.Email!))
            .ReturnsAsync(user);

        _userManagerMock.Setup(x => x.GeneratePasswordResetTokenAsync(user))
            .ReturnsAsync("reset-token");

        var handler = new RequestPasswordResetHandler(_userManagerMock.Object, _loggerMock.Object);

        var command = new RequestPasswordResetCommand { Email = user.Email! };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _userManagerMock.Verify(x => x.GeneratePasswordResetTokenAsync(user), Times.Once);
    }

    [Fact]
    public void Validators_ShouldValidateCommands()
    {
        var requestValidator = new RequestPasswordResetValidator();
        var resetValidator = new ResetPasswordValidator();

        requestValidator.TestValidate(new RequestPasswordResetCommand { Email = "" })
            .ShouldHaveValidationErrorFor(c => c.Email);

        resetValidator.TestValidate(new ResetPasswordCommand
            {
                Email = "",
                Token = "",
                NewPassword = "short"
            })
            .ShouldHaveValidationErrorFor(c => c.Email);
    }
}

