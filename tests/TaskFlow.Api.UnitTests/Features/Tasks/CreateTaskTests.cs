using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using Moq;
using Microsoft.AspNetCore.SignalR;
using TaskFlow.Domain.Entities;
using TaskFlow.Features.Tasks.CreateTask;
using TaskFlow.Hubs;
using TaskFlow.Infrastructure.Persistence;
using Xunit;
using TaskStatus = TaskFlow.Domain.Entities.TaskStatus;

namespace TaskFlow.Api.UnitTests.Features.Tasks;

public class CreateTaskTests
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<IHubContext<TasksHub>> _hubContextMock;
    private readonly Mock<IHubClients> _hubClientsMock;
    private readonly Mock<IClientProxy> _clientProxyMock;
    private readonly CreateTaskHandler _handler;
    private readonly CreateTaskValidator _validator;

    public CreateTaskTests()
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
        _handler = new CreateTaskHandler(_dbContext, _hubContextMock.Object);
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
            Priority = 1,
            UserId = "user-123"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("Test Task");
        result.Description.Should().Be("Description");
        result.Priority.Should().Be(1);

        var task = await _dbContext.Tasks.FirstAsync();
        task.Title.Should().Be("Test Task");
        task.UserId.Should().Be("user-123");
    }

    [Fact]
    public async Task Handle_ShouldSendSignalRNotification_WhenTaskCreated()
    {
        // Arrange
        var command = new CreateTaskCommand
        {
            Title = "Test Task",
            Description = "Description",
            Priority = 1,
            UserId = "user-123"
        };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _clientProxyMock.Verify(c => c.SendCoreAsync(
            "TaskCreated",
            It.Is<object[]>(o => o.Length == 1),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldSetDefaultStatus_WhenStatusNotProvided()
    {
        // Arrange
        var command = new CreateTaskCommand
        {
            Title = "Test Task",
            UserId = "user-123"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Status.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ShouldSetSpecifiedStatus_WhenStatusProvided()
    {
        // Arrange
        var command = new CreateTaskCommand
        {
            Title = "Test Task",
            UserId = "user-123",
            Status = 1
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Status.Should().Be(1);
    }

    [Fact]
    public async Task Handle_ShouldIncrementOrderIndex()
    {
        // Arrange
        var userId = "user-123";
        
        _dbContext.Tasks.Add(new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = "Existing Task",
            Priority = TaskPriority.Medium,
            Status = TaskStatus.ToDo,
            OrderIndex = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            UserId = userId
        });
        await _dbContext.SaveChangesAsync();

        var command = new CreateTaskCommand
        {
            Title = "New Task",
            UserId = userId
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.OrderIndex.Should().Be(1);
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

    [Fact]
    public void Validator_ShouldHaveError_WhenTitleIsNull()
    {
        // Arrange
        var command = new CreateTaskCommand
        {
            Title = null!,
            UserId = "user-123"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void Validator_ShouldHaveError_WhenTitleExceedsMaxLength()
    {
        // Arrange
        var command = new CreateTaskCommand
        {
            Title = new string('A', 501),
            UserId = "user-123"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void Validator_ShouldHaveError_WhenPriorityOutOfRange()
    {
        // Arrange
        var command = new CreateTaskCommand
        {
            Title = "Test Task",
            UserId = "user-123",
            Priority = 5
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Priority);
    }

    [Fact]
    public void Validator_ShouldPass_WhenAllFieldsAreValid()
    {
        // Arrange
        var command = new CreateTaskCommand
        {
            Title = "Valid Task",
            Description = "Valid Description",
            Priority = 2,
            UserId = "user-123"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
