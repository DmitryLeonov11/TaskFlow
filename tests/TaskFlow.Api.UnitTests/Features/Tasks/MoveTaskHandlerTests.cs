using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Domain.Entities;
using TaskFlow.Features.Tasks.MoveTask;
using TaskFlow.Infrastructure.Persistence;
using Xunit;

namespace TaskFlow.Api.UnitTests.Features.Tasks;

public class MoveTaskHandlerTests
{
    private readonly ApplicationDbContext _dbContext;
    private readonly MoveTaskHandler _handler;

    public MoveTaskHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApplicationDbContext(options);
        _handler = new MoveTaskHandler(_dbContext);
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
}

