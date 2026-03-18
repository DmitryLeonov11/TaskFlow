using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Domain.Entities;
using TaskFlow.Features.Tasks.GetTasks;
using TaskFlow.Infrastructure.Persistence;
using Xunit;
using TaskStatus = TaskFlow.Domain.Entities.TaskStatus;

namespace TaskFlow.Api.UnitTests.Features.Tasks;

public class GetTasksTests
{
    private readonly ApplicationDbContext _dbContext;
    private readonly GetTasksHandler _handler;

    public GetTasksTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApplicationDbContext(options);
        _handler = new GetTasksHandler(_dbContext);
    }

    [Fact]
    public async Task Handle_ShouldReturnAllTasksForUser_WhenNoFiltersApplied()
    {
        var userId = "user-123";
        
        _dbContext.Tasks.AddRange(
            new TaskItem { Id = Guid.NewGuid(), Title = "Task 1", Priority = TaskPriority.Medium, Status = TaskStatus.ToDo, OrderIndex = 0, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, UserId = userId },
            new TaskItem { Id = Guid.NewGuid(), Title = "Task 2", Priority = TaskPriority.High, Status = TaskStatus.InProgress, OrderIndex = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, UserId = userId }
        );
        await _dbContext.SaveChangesAsync();

        var query = new GetTasksQuery { UserId = userId, PageNumber = 1, PageSize = 10 };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Tasks.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenUserHasNoTasks()
    {
        var query = new GetTasksQuery { UserId = "user-with-no-tasks", PageNumber = 1, PageSize = 10 };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Tasks.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ShouldFilterByStatus_WhenStatusProvided()
    {
        var userId = "user-123";
        
        _dbContext.Tasks.AddRange(
            new TaskItem { Id = Guid.NewGuid(), Title = "ToDo", Priority = TaskPriority.Medium, Status = TaskStatus.ToDo, OrderIndex = 0, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, UserId = userId },
            new TaskItem { Id = Guid.NewGuid(), Title = "InProgress", Priority = TaskPriority.High, Status = TaskStatus.InProgress, OrderIndex = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, UserId = userId }
        );
        await _dbContext.SaveChangesAsync();

        var query = new GetTasksQuery { UserId = userId, Status = (int)TaskStatus.InProgress, PageNumber = 1, PageSize = 10 };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Tasks.Should().HaveCount(1);
        result.Tasks.First().Title.Should().Be("InProgress");
    }

    [Fact]
    public async Task Handle_ShouldApplyPagination_WhenPageSizeSpecified()
    {
        var userId = "user-123";
        for (int i = 0; i < 15; i++)
        {
            _dbContext.Tasks.Add(new TaskItem { Id = Guid.NewGuid(), Title = $"Task {i}", Priority = TaskPriority.Medium, Status = TaskStatus.ToDo, OrderIndex = i, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, UserId = userId });
        }
        await _dbContext.SaveChangesAsync();

        var query = new GetTasksQuery { UserId = userId, PageNumber = 1, PageSize = 5 };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Tasks.Should().HaveCount(5);
        result.TotalCount.Should().Be(15);
    }

    [Fact]
    public async Task Handle_ShouldSearchByTitle_WhenSearchTermProvided()
    {
        var userId = "user-123";
        _dbContext.Tasks.AddRange(
            new TaskItem { Id = Guid.NewGuid(), Title = "Important Task", Description = "This is important", Priority = TaskPriority.Medium, Status = TaskStatus.ToDo, OrderIndex = 0, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, UserId = userId },
            new TaskItem { Id = Guid.NewGuid(), Title = "Regular Task", Description = "Nothing special", Priority = TaskPriority.Low, Status = TaskStatus.ToDo, OrderIndex = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, UserId = userId }
        );
        await _dbContext.SaveChangesAsync();

        var query = new GetTasksQuery { UserId = userId, SearchTerm = "important", PageNumber = 1, PageSize = 10 };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Tasks.Should().HaveCount(1);
        result.Tasks.First().Title.Should().Contain("Important");
    }

    [Fact]
    public async Task Handle_ShouldNotReturnOtherUsersTasks()
    {
        _dbContext.Tasks.AddRange(
            new TaskItem { Id = Guid.NewGuid(), Title = "User 1 Task", Priority = TaskPriority.Medium, Status = TaskStatus.ToDo, OrderIndex = 0, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, UserId = "user-1" },
            new TaskItem { Id = Guid.NewGuid(), Title = "User 2 Task", Priority = TaskPriority.High, Status = TaskStatus.InProgress, OrderIndex = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, UserId = "user-2" }
        );
        await _dbContext.SaveChangesAsync();

        var query = new GetTasksQuery { UserId = "user-1", PageNumber = 1, PageSize = 10 };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Tasks.Should().HaveCount(1);
        result.Tasks.First().Title.Should().Be("User 1 Task");
    }
}
