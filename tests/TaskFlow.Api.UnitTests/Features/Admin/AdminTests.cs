using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Domain.Entities;
using TaskFlow.Features.Admin.GetAllTasks;
using TaskFlow.Features.Admin.RestoreTask;
using TaskFlow.Infrastructure.Persistence;
using Xunit;
using TaskStatus = TaskFlow.Domain.Entities.TaskStatus;

namespace TaskFlow.Api.UnitTests.Features.Admin;

public class AdminTests
{
    private readonly ApplicationDbContext _dbContext;

    public AdminTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApplicationDbContext(options);
    }

    private TaskItem MakeTask(string userId, bool isDeleted = false) => new TaskItem
    {
        Id = Guid.NewGuid(),
        Title = "Task",
        Priority = TaskPriority.Medium,
        Status = TaskStatus.ToDo,
        OrderIndex = 0,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow,
        UserId = userId,
        IsDeleted = isDeleted,
        DeletedAt = isDeleted ? DateTime.UtcNow : null
    };

    // ── GetAllTasks ───────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllTasks_ShouldIncludeSoftDeleted_WhenFlagIsTrue()
    {
        _dbContext.Tasks.AddRange(MakeTask("u1"), MakeTask("u2", isDeleted: true));
        await _dbContext.SaveChangesAsync();

        var handler = new GetAllTasksHandler(_dbContext);
        var result = await handler.Handle(new GetAllTasksQuery { IncludeDeleted = true }, CancellationToken.None);

        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task GetAllTasks_ShouldExcludeSoftDeleted_WhenFlagIsFalse()
    {
        _dbContext.Tasks.AddRange(MakeTask("u1"), MakeTask("u2", isDeleted: true));
        await _dbContext.SaveChangesAsync();

        var handler = new GetAllTasksHandler(_dbContext);
        var result = await handler.Handle(new GetAllTasksQuery { IncludeDeleted = false }, CancellationToken.None);

        result.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task GetAllTasks_ShouldReturnTasksAcrossAllUsers()
    {
        _dbContext.Tasks.AddRange(MakeTask("alice"), MakeTask("bob"), MakeTask("carol"));
        await _dbContext.SaveChangesAsync();

        var handler = new GetAllTasksHandler(_dbContext);
        var result = await handler.Handle(new GetAllTasksQuery { IncludeDeleted = false }, CancellationToken.None);

        result.TotalCount.Should().Be(3);
    }

    // ── RestoreTask ───────────────────────────────────────────────────────────

    [Fact]
    public async Task RestoreTask_ShouldClearIsDeletedFlag()
    {
        var task = MakeTask("u1", isDeleted: true);
        _dbContext.Tasks.Add(task);
        await _dbContext.SaveChangesAsync();

        var handler = new RestoreTaskHandler(_dbContext);
        var result = await handler.Handle(new RestoreTaskCommand { TaskId = task.Id }, CancellationToken.None);

        result.Should().NotBeNull();

        var restored = await _dbContext.Tasks.FindAsync(task.Id);
        restored.Should().NotBeNull();
        restored!.IsDeleted.Should().BeFalse();
        restored.DeletedAt.Should().BeNull();
    }

    [Fact]
    public async Task RestoreTask_ShouldThrow_WhenTaskDoesNotExist()
    {
        var handler = new RestoreTaskHandler(_dbContext);
        await handler.Invoking(h => h.Handle(new RestoreTaskCommand { TaskId = Guid.NewGuid() }, CancellationToken.None))
            .Should().ThrowAsync<KeyNotFoundException>();
    }
}
