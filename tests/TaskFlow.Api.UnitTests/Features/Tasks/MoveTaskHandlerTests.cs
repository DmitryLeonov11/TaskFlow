using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Microsoft.AspNetCore.SignalR;
using TaskFlow.Domain.Entities;
using TaskFlow.Features.Tasks.MoveTask;
using TaskFlow.Hubs;
using TaskFlow.Infrastructure.Persistence;
using Xunit;
using TaskStatus = TaskFlow.Domain.Entities.TaskStatus;

namespace TaskFlow.Api.UnitTests.Features.Tasks;

public class MoveTaskHandlerTests
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<IHubContext<TasksHub>> _hubContextMock;
    private readonly Mock<IHubClients> _hubClientsMock;
    private readonly Mock<IClientProxy> _clientProxyMock;
    private readonly MoveTaskHandler _handler;

    public MoveTaskHandlerTests()
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
        _handler = new MoveTaskHandler(_dbContext, _hubContextMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldMoveTask_ToNewStatusAndOrder()
    {
        // Arrange
        var userId = "user-1";
        var taskId = Guid.NewGuid();

        _dbContext.Tasks.AddRange(
            new TaskItem
            {
                Id = taskId,
                Title = "Task 1",
                Priority = TaskPriority.Medium,
                Status = TaskStatus.ToDo,
                OrderIndex = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                UserId = userId
            },
            new TaskItem
            {
                Id = Guid.NewGuid(),
                Title = "Task 2",
                Priority = TaskPriority.Medium,
                Status = TaskStatus.InProgress,
                OrderIndex = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                UserId = userId
            }
        );
        await _dbContext.SaveChangesAsync();

        var command = new MoveTaskCommand
        {
            TaskId = taskId,
            UserId = userId,
            NewStatus = (int)TaskStatus.InProgress,
            NewOrderIndex = 1
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Status.Should().Be((int)TaskStatus.InProgress);
        result.OrderIndex.Should().Be(1);

        var movedTask = await _dbContext.Tasks.FirstAsync(t => t.Id == taskId);
        movedTask.Status.Should().Be(TaskStatus.InProgress);
        movedTask.OrderIndex.Should().Be(1);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenTaskNotFound()
    {
        // Arrange
        var command = new MoveTaskCommand
        {
            TaskId = Guid.NewGuid(),
            UserId = "user-1",
            NewStatus = (int)TaskStatus.InProgress,
            NewOrderIndex = 0
        };

        // Act & Assert
        await _handler.Invoking(h => h.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Task with id * not found");
    }

    [Fact]
    public async Task Handle_ShouldSendSignalRNotification_WhenTaskMoved()
    {
        // Arrange
        var userId = "user-1";
        var taskId = Guid.NewGuid();

        _dbContext.Tasks.Add(new TaskItem
        {
            Id = taskId,
            Title = "Task 1",
            Priority = TaskPriority.Medium,
            Status = TaskStatus.ToDo,
            OrderIndex = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            UserId = userId
        });
        await _dbContext.SaveChangesAsync();

        var command = new MoveTaskCommand
        {
            TaskId = taskId,
            UserId = userId,
            NewStatus = (int)TaskStatus.InProgress,
            NewOrderIndex = 0
        };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _clientProxyMock.Verify(c => c.SendCoreAsync(
            "TaskMoved",
            It.Is<object[]>(o => o.Length == 1),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
