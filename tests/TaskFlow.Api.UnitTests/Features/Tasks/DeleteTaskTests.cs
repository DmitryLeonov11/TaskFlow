using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Domain.Entities;
using TaskFlow.Features.Tasks.DeleteTask;
using TaskFlow.Infrastructure.Persistence;
using Xunit;
using TaskStatus = TaskFlow.Domain.Entities.TaskStatus;

namespace TaskFlow.Api.UnitTests.Features.Tasks;

public class DeleteTaskTests
{
    private readonly ApplicationDbContext _dbContext;
    private readonly DeleteTaskHandler _handler;

    public DeleteTaskTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApplicationDbContext(options);
        _handler = new DeleteTaskHandler(_dbContext);
    }

    [Fact]
    public async Task Handle_ShouldDeleteTask_WhenTaskExists()
    {
        var userId = "user-123";
        var taskId = Guid.NewGuid();
        
        var task = new TaskItem
        {
            Id = taskId,
            Title = "Task to Delete",
            Priority = TaskPriority.Medium,
            Status = TaskStatus.ToDo,
            OrderIndex = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            UserId = userId
        };
        
        _dbContext.Tasks.Add(task);
        await _dbContext.SaveChangesAsync();

        var command = new DeleteTaskCommand { TaskId = taskId, UserId = userId };
        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().BeTrue();
        var deletedTask = await _dbContext.Tasks.FindAsync(taskId);
        deletedTask.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenTaskNotFound()
    {
        var command = new DeleteTaskCommand { TaskId = Guid.NewGuid(), UserId = "user-123" };
        await _handler.Invoking(h => h.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Task with id * not found");
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenTaskBelongsToDifferentUser()
    {
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = "Other User's Task",
            Priority = TaskPriority.Medium,
            Status = TaskStatus.ToDo,
            OrderIndex = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            UserId = "other-user"
        };
        
        _dbContext.Tasks.Add(task);
        await _dbContext.SaveChangesAsync();

        var command = new DeleteTaskCommand { TaskId = task.Id, UserId = "different-user" };
        await _handler.Invoking(h => h.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<KeyNotFoundException>();
    }
}
