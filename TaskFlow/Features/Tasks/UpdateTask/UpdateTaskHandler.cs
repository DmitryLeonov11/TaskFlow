using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.DTOs;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.Features.Tasks.UpdateTask;

public class UpdateTaskHandler : IRequestHandler<UpdateTaskCommand, TaskItemDto>
{
    private readonly ApplicationDbContext _dbContext;

    public UpdateTaskHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TaskItemDto> Handle(UpdateTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await _dbContext.Tasks
            .Include(t => t.TaskTags)
            .ThenInclude(tt => tt.Tag)
            .Include(t => t.Comments)
            .FirstOrDefaultAsync(t => t.Id == request.TaskId && t.UserId == request.UserId, cancellationToken)
            ?? throw new KeyNotFoundException($"Task with id {request.TaskId} not found");

        if (!string.IsNullOrWhiteSpace(request.Title))
            task.Title = request.Title;

        if (request.Description != null)
            task.Description = request.Description;

        if (request.Priority.HasValue)
            task.Priority = (TaskPriority)request.Priority.Value;

        if (request.Status.HasValue)
            task.Status = (TaskStatus)request.Status.Value;

        if (request.Deadline != null)
            task.Deadline = request.Deadline;

        task.UpdatedAt = DateTime.UtcNow;

        // Update tags
        if (request.TagIds != null)
        {
            _dbContext.TaskTags.RemoveRange(task.TaskTags);

            var tags = await _dbContext.Tags
                .Where(t => request.TagIds.Contains(t.Id) && t.UserId == request.UserId)
                .ToListAsync(cancellationToken);

            foreach (var tag in tags)
            {
                task.TaskTags.Add(new TaskTag { TaskId = task.Id, TagId = tag.Id });
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToDto(task);
    }

    private static TaskItemDto MapToDto(TaskItem task)
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
            task.TaskTags.Select(tt => new TagDto(tt.Tag.Id, tt.Tag.Name, tt.Tag.Color)).ToList(),
            task.Comments.Select(c => new TaskCommentDto(c.Id, c.UserId, c.Content, c.CreatedAt)).ToList());
    }
}
