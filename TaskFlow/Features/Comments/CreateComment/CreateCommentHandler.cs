using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.DTOs;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Persistence;
using TaskFlow.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace TaskFlow.Features.Comments.CreateComment;

public class CreateCommentHandler : IRequestHandler<CreateCommentCommand, TaskCommentDto>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IHubContext<TasksHub> _hubContext;

    public CreateCommentHandler(
        ApplicationDbContext dbContext,
        IHubContext<TasksHub> hubContext)
    {
        _dbContext = dbContext;
        _hubContext = hubContext;
    }

    public async Task<TaskCommentDto> Handle(CreateCommentCommand request, CancellationToken cancellationToken)
    {
        var task = await _dbContext.Tasks
            .Include(t => t.TaskTags)
            .ThenInclude(tt => tt.Tag)
            .FirstOrDefaultAsync(t => t.Id == request.TaskId, cancellationToken);

        if (task == null)
        {
            throw new InvalidOperationException("Task not found");
        }

        if (task.UserId != request.UserId)
        {
            throw new UnauthorizedAccessException("You can only comment on your own tasks");
        }

        var comment = new TaskComment
        {
            Id = Guid.NewGuid(),
            TaskId = request.TaskId,
            UserId = request.UserId,
            Content = request.Content,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext.TaskComments.Add(comment);

        // Create notification
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            Type = NotificationType.CommentAdded,
            Message = $"Comment added to task: {task.Title}",
            RelatedTaskId = task.Id,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Notifications.Add(notification);

        await _dbContext.SaveChangesAsync(cancellationToken);

        // SignalR notification
        await _hubContext.Clients.User(request.UserId).SendAsync("CommentAdded", new
        {
            TaskId = request.TaskId,
            CommentId = comment.Id,
            Content = comment.Content,
            CreatedAt = comment.CreatedAt
        }, cancellationToken);

        return new TaskCommentDto(
            comment.Id,
            comment.UserId,
            comment.Content,
            comment.CreatedAt
        );
    }
}
