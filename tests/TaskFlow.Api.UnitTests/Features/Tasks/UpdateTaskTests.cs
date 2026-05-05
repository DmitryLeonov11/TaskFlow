using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using Moq;
using Microsoft.AspNetCore.SignalR;
using TaskFlow.Domain.Entities;
using TaskFlow.Features.Tasks.UpdateTask;
using TaskFlow.Hubs;
using TaskFlow.Infrastructure.Persistence;
using TaskFlow.Application.Common;
using Xunit;
using TaskStatus = TaskFlow.Domain.Entities.TaskStatus;

namespace TaskFlow.Api.UnitTests.Features.Tasks;

public class UpdateTaskTests
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<IHubContext<TasksHub>> _hubContextMock;
    private readonly Mock<IHubClients> _hubClientsMock;
    private readonly Mock<IClientProxy> _clientProxyMock;
    private readonly UpdateTaskHandler _handler;
    private readonly UpdateTaskValidator _validator;

    public UpdateTaskTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApplicationDbContext(options);
        _hubContextMock = new Mock<IHubContext<TasksHub>>();
        _hubClientsMock = new Mock<IHubClients>();
        _clientProxyMock = new Mock<IClientProxy>();
        _hubContextMock.Setup(h => h.Clients).Returns(_hubClientsMock.Object);
        _hubClientsMock.Setup(c => c.User(It.IsAny<string>())).Returns(_clientProxyMock.Object);
        _handler = new UpdateTaskHandler(_dbContext, _hubContextMock.Object);
        _validator = new UpdateTaskValidator();
    }

    [Fact]
    public async Task Handle_ShouldUpdateTask_WhenTaskExists()
    {
        // Arrange
        var userId = "user-123";
        var taskId = Guid.NewGuid();
        
        var task = new TaskItem
        {
            Id = taskId,
            Title = "Original Title",
            Description = "Original Description",
            Priority = TaskPriority.Low,
            Status = TaskStatus.ToDo,
            OrderIndex = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            UserId = userId
        };
        
        _dbContext.Tasks.Add(task);
        await _dbContext.SaveChangesAsync();

        var command = new UpdateTaskCommand
        {
            TaskId = taskId,
            UserId = userId,
            Title = "Updated Title",
            Description = "Updated Description",
            Priority = 3,
            Status = 1
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("Updated Title");
        result.Description.Should().Be("Updated Description");
        result.Priority.Should().Be(3);
        result.Status.Should().Be(1);

        var updatedTask = await _dbContext.Tasks.FirstAsync(t => t.Id == taskId);
        updatedTask.Title.Should().Be("Updated Title");
        updatedTask.Priority.Should().Be(TaskPriority.Urgent);
        updatedTask.Status.Should().Be(TaskStatus.InProgress);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenTaskNotFound()
    {
        // Arrange
        var command = new UpdateTaskCommand
        {
            TaskId = Guid.NewGuid(),
            UserId = "user-123",
            Title = "Updated Title"
        };

        // Act & Assert
        await _handler.Invoking(h => h.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Task with id * not found");
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenTaskBelongsToDifferentUser()
    {
        // Arrange
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = "Task",
            Priority = TaskPriority.Medium,
            Status = TaskStatus.ToDo,
            OrderIndex = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            UserId = "other-user"
        };
        
        _dbContext.Tasks.Add(task);
        await _dbContext.SaveChangesAsync();

        var command = new UpdateTaskCommand
        {
            TaskId = task.Id,
            UserId = "different-user",
            Title = "Updated Title"
        };

        // Act & Assert
        await _handler.Invoking(h => h.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_ShouldSendSignalRNotification_WhenTaskUpdated()
    {
        // Arrange
        var userId = "user-123";
        var taskId = Guid.NewGuid();
        
        var task = new TaskItem
        {
            Id = taskId,
            Title = "Original Title",
            Priority = TaskPriority.Medium,
            Status = TaskStatus.ToDo,
            OrderIndex = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            UserId = userId
        };
        
        _dbContext.Tasks.Add(task);
        await _dbContext.SaveChangesAsync();

        var command = new UpdateTaskCommand
        {
            TaskId = taskId,
            UserId = userId,
            Title = new Optional<string>("Updated Title")
        };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _clientProxyMock.Verify(c => c.SendCoreAsync(
            "TaskUpdated",
            It.Is<object[]>(o => o.Length == 1),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public void Validator_ShouldHaveError_WhenTaskIdIsEmpty()
    {
        var command = new UpdateTaskCommand
        {
            TaskId = Guid.Empty,
            UserId = "user-123"
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.TaskId);
    }

    [Fact]
    public void Validator_ShouldHaveError_WhenTitleExceedsMaxLength()
    {
        var command = new UpdateTaskCommand
        {
            TaskId = Guid.NewGuid(),
            UserId = "user-123",
            Title = new Optional<string>(new string('A', 501))
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void Validator_ShouldHaveError_WhenDescriptionExceedsMaxLength()
    {
        var command = new UpdateTaskCommand
        {
            TaskId = Guid.NewGuid(),
            UserId = "user-123",
            Description = new Optional<string?>(new string('A', 5001))
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Validator_ShouldHaveError_WhenPriorityOutOfRange()
    {
        var command = new UpdateTaskCommand
        {
            TaskId = Guid.NewGuid(),
            UserId = "user-123",
            Priority = new Optional<int>(5)
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Priority);
    }

    [Fact]
    public void Validator_ShouldHaveError_WhenStatusOutOfRange()
    {
        var command = new UpdateTaskCommand
        {
            TaskId = Guid.NewGuid(),
            UserId = "user-123",
            Status = 5
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Status);
    }

    [Fact]
    public void Validator_ShouldHaveError_WhenDeadlineIsInPast()
    {
        var command = new UpdateTaskCommand
        {
            TaskId = Guid.NewGuid(),
            UserId = "user-123",
            Deadline = DateTime.UtcNow.AddDays(-1)
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Deadline);
    }

    [Fact]
    public void Validator_ShouldPass_WhenAllFieldsAreValid()
    {
        var command = new UpdateTaskCommand
        {
            TaskId = Guid.NewGuid(),
            UserId = "user-123",
            Title = "Valid Title",
            Description = "Valid Description",
            Priority = new Optional<int>(2),
            Status = new Optional<int>(1),
            Deadline = new Optional<DateTime?>(DateTime.UtcNow.AddDays(7))
        };

        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
