using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.DTOs;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.Features.Tasks.CreateTask;

public class CreateTaskHandler : IRequestHandler<CreateTaskCommand, TaskItemDto>
{
    private readonly ApplicationDbContext _dbContext;

    public CreateTaskHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TaskItemDto> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
    {
        var taskItem = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            Priority = (TaskPriority)request.Priority,
            Status = TaskStatus.ToDo,
            Deadline = request.Deadline,
            OrderIndex = await GetNextOrderIndex(request.UserId, TaskStatus.ToDo, cancellationToken),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            UserId = request.UserId
        };

        _dbContext.Tasks.Add(taskItem);

        // Add tags if provided
        if (request.TagIds?.Count > 0)
        {
            var tags = await _dbContext.Tags
                .Where(t => request.TagIds.Contains(t.Id) && t.UserId == request.UserId)
                .ToListAsync(cancellationToken);

            foreach (var tag in tags)
            {
                taskItem.TaskTags.Add(new TaskTag { TaskId = taskItem.Id, TagId = tag.Id });
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToDto(taskItem, new List<Tag>(), new List<TaskAttachment>());
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
            attachments.Select(a => new TaskAttachmentDto(a.Id, a.FileName, a.FileSize, a.UploadedAt)).ToList());
    }
}
