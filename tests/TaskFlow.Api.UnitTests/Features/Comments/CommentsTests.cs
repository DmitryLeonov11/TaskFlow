using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Microsoft.AspNetCore.SignalR;
using TaskFlow.Domain.Entities;
using TaskFlow.Features.Comments.CreateComment;
using TaskFlow.Features.Comments.DeleteComment;
using TaskFlow.Hubs;
using TaskFlow.Infrastructure.Persistence;
using Xunit;
using TaskStatus = TaskFlow.Domain.Entities.TaskStatus;

namespace TaskFlow.Api.UnitTests.Features.Comments;

public class CreateCommentTests
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<IHubContext<TasksHub>> _hubContextMock;
    private readonly Mock<IHubClients> _hubClientsMock;
    private readonly Mock<IClientProxy> _clientProxyMock;
    private readonly CreateCommentHandler _handler;

    public CreateCommentTests()
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
        _handler = new CreateCommentHandler(_dbContext, _hubContextMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateComment_WhenTaskExists()
    {
        var userId = "user-123";
        var taskId = Guid.NewGuid();
        
        var task = new TaskItem
        {
            Id = taskId,
            Title = "Task with Comment",
            Priority = TaskPriority.Medium,
            Status = TaskStatus.ToDo,
            OrderIndex = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            UserId = userId
        };
        
        _dbContext.Tasks.Add(task);
        await _dbContext.SaveChangesAsync();

        var command = new CreateCommentCommand { TaskId = taskId, UserId = userId, Content = "This is a test comment" };
        var result = await _handler.Handle(command, CancellationToken.None);

        result.Content.Should().Be("This is a test comment");
        var comment = await _dbContext.TaskComments.FirstAsync();
        comment.Content.Should().Be("This is a test comment");
    }

    [Fact]
    public async Task Handle_ShouldCreateNotification_WhenCommentAdded()
    {
        var userId = "user-123";
        var taskId = Guid.NewGuid();
        
        var task = new TaskItem { Id = taskId, Title = "Task Title", Priority = TaskPriority.Medium, Status = TaskStatus.ToDo, OrderIndex = 0, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, UserId = userId };
        _dbContext.Tasks.Add(task);
        await _dbContext.SaveChangesAsync();

        var command = new CreateCommentCommand { TaskId = taskId, UserId = userId, Content = "Test comment" };
        await _handler.Handle(command, CancellationToken.None);

        var notification = await _dbContext.Notifications.FirstAsync();
        notification.Type.Should().Be(NotificationType.CommentAdded);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenTaskNotFound()
    {
        var command = new CreateCommentCommand { TaskId = Guid.NewGuid(), UserId = "user-123", Content = "Test" };
        await _handler.Invoking(h => h.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Task not found");
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenUserDoesNotOwnTask()
    {
        var task = new TaskItem { Id = Guid.NewGuid(), Title = "Task", Priority = TaskPriority.Medium, Status = TaskStatus.ToDo, OrderIndex = 0, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, UserId = "other-user" };
        _dbContext.Tasks.Add(task);
        await _dbContext.SaveChangesAsync();

        var command = new CreateCommentCommand { TaskId = task.Id, UserId = "different-user", Content = "Test" };
        await _handler.Invoking(h => h.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("You can only comment on your own tasks");
    }
}

public class DeleteCommentTests
{
    private readonly ApplicationDbContext _dbContext;
    private readonly DeleteCommentHandler _handler;

    public DeleteCommentTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApplicationDbContext(options);
        _handler = new DeleteCommentHandler(_dbContext);
    }

    [Fact]
    public async Task Handle_ShouldDeleteComment_WhenCommentExists()
    {
        var userId = "user-123";
        var commentId = Guid.NewGuid();
        
        var comment = new TaskComment { Id = commentId, TaskId = Guid.NewGuid(), UserId = userId, Content = "Comment", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        _dbContext.TaskComments.Add(comment);
        await _dbContext.SaveChangesAsync();

        var command = new DeleteCommentCommand { CommentId = commentId, UserId = userId };
        await _handler.Handle(command, CancellationToken.None);

        var deletedComment = await _dbContext.TaskComments.FindAsync(commentId);
        deletedComment.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenCommentNotFound()
    {
        var command = new DeleteCommentCommand { CommentId = Guid.NewGuid(), UserId = "user-123" };
        await _handler.Invoking(h => h.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Comment not found");
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenUserDoesNotOwnComment()
    {
        var comment = new TaskComment { Id = Guid.NewGuid(), TaskId = Guid.NewGuid(), UserId = "other-user", Content = "Comment", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        _dbContext.TaskComments.Add(comment);
        await _dbContext.SaveChangesAsync();

        var command = new DeleteCommentCommand { CommentId = comment.Id, UserId = "different-user" };
        await _handler.Invoking(h => h.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("You can only delete your own comments");
    }
}
