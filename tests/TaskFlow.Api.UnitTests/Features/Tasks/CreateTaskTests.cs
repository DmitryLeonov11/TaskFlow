using FluentAssertions;
using FluentValidation.TestHelper;
using MediatR;
using Moq;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Interfaces;
using TaskFlow.Features.Tasks.CreateTask;
using Xunit;

namespace TaskFlow.Api.UnitTests.Features.Tasks;

public class CreateTaskTests
{
    private readonly Mock<ITaskRepository> _taskRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CreateTaskHandler _handler;
    private readonly CreateTaskValidator _validator;

    public CreateTaskTests()
    {
        _taskRepositoryMock = new Mock<ITaskRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new CreateTaskHandler(_taskRepositoryMock.Object, _unitOfWorkMock.Object);
        _validator = new CreateTaskValidator();
    }

    [Fact]
    public async Task Handle_ShouldCreateTask_WhenRequestIsValid()
    {
        // Arrange
        var command = new CreateTaskCommand
        {
            Title = "Test Task",
            Description = "Description",
            Priority = 1, // Medium
            UserId = "user-123"
        };

        _taskRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("Test Task");
        _taskRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void Validator_ShouldHaveError_WhenTitleIsEmpty()
    {
        // Arrange
        var command = new CreateTaskCommand
        {
            Title = string.Empty,
            UserId = "user-123"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }
}
