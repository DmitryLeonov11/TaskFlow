using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.DTOs;
using TaskFlow.Domain.Entities;
using TaskFlow.Hubs;
using TaskFlow.Infrastructure.Persistence;
using TaskStatus = TaskFlow.Domain.Entities.TaskStatus;

namespace TaskFlow.Features.Tasks.CreateTask;

public class CreateTaskHandler : IRequestHandler<CreateTaskCommand, TaskItemDto>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IHubContext<TasksHub> _tasksHub;

    public CreateTaskHandler(ApplicationDbContext dbContext, IHubContext<TasksHub> tasksHub)
    {
        _dbContext = dbContext;
        _tasksHub = tasksHub;
    }

    public async Task<TaskItemDto> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
    {
        var targetStatus = (TaskStatus)(request.Status ?? 0);

        var taskItem = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            Priority = (TaskPriority)request.Priority,
            Status = targetStatus,
            Deadline = request.Deadline.HasValue ? DateTime.SpecifyKind(request.Deadline.Value, DateTimeKind.Utc) : null,
            OrderIndex = request.ParentTaskId.HasValue ? 0 : await GetNextOrderIndex(request.UserId, targetStatus, cancellationToken),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            UserId = request.UserId,
            ProjectId = request.ProjectId,
            ParentTaskId = request.ParentTaskId
        };

        _dbContext.Tasks.Add(taskItem);

        // Add tags if provided
        List<Tag> addedTags = new();
        if (request.TagIds?.Count > 0)
        {
            var tags = await _dbContext.Tags
                .Where(t => request.TagIds.Contains(t.Id) && t.UserId == request.UserId)
                .ToListAsync(cancellationToken);

            foreach (var tag in tags)
            {
                taskItem.TaskTags.Add(new TaskTag { TaskId = taskItem.Id, TagId = tag.Id });
                addedTags.Add(tag);
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        var dto = MapToDto(taskItem, addedTags, new List<TaskAttachment>());

        await _tasksHub.Clients.User(request.UserId).SendAsync("TaskCreated", dto, cancellationToken);

        return dto;
    }

    private async Task<int> GetNextOrderIndex(string userId, TaskStatus status, CancellationToken cancellationToken)
    {
        var maxOrder = await _dbContext.Tasks
            .Where(t => t.UserId == userId && t.Status == status)
            .MaxAsync(t => (int?)t.OrderIndex, cancellationToken) ?? 0;

        return maxOrder + 1;
    }

    private static TaskItemDto MapToDto(TaskItem task, IReadOnlyList<Tag> tags, IReadOnlyList<TaskAttachment> attachments)
    {
        return new TaskItemDto(
            task.Id,
            task.Title,
            task.Description,
            (int)task.Priority,
            (int)task.Status,
            task.Deadline,
            task.OrderIndex,
            task.CreatedAt,
            task.UpdatedAt,
            tags.Select(t => new TagDto(t.Id, t.Name, t.Color)).ToList(),
            new List<TaskCommentDto>(),
            attachments.Select(a => new TaskAttachmentDto(a.Id, a.FileName, a.FileSize, a.UploadedAt)).ToList(),
            task.ProjectId,
            task.ParentTaskId,
            new List<SubtaskDto>());
    }
}