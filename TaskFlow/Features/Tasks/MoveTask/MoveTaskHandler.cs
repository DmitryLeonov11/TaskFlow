using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.DTOs;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.Features.Tasks.MoveTask;

public class MoveTaskHandler : IRequestHandler<MoveTaskCommand, TaskItemDto>
{
    private readonly ApplicationDbContext _dbContext;

    public MoveTaskHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TaskItemDto> Handle(MoveTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await _dbContext.Tasks
            .Include(t => t.TaskTags)
            .ThenInclude(tt => tt.Tag)
            .Include(t => t.Comments)
            .Include(t => t.Attachments)
            .FirstOrDefaultAsync(t => t.Id == request.TaskId && t.UserId == request.UserId, cancellationToken)
            ?? throw new KeyNotFoundException($"Task with id {request.TaskId} not found");

        var newStatus = (TaskStatus)request.NewStatus;
        var oldStatus = task.Status;

        // If status changed, reorder tasks in the old column
        if (oldStatus != newStatus)
        {
            var tasksInOldColumn = await _dbContext.Tasks
                .Where(t => t.UserId == request.UserId && t.Status == oldStatus && t.Id != request.TaskId)
                .OrderBy(t => t.OrderIndex)
                .ToListAsync(cancellationToken);

            for (int i = 0; i < tasksInOldColumn.Count; i++)
            {
                tasksInOldColumn[i].OrderIndex = i;
            }

            task.Status = newStatus;
        }

        // Reorder tasks in the new column
        var tasksInNewColumn = await _dbContext.Tasks
            .Where(t => t.UserId == request.UserId && t.Status == newStatus && t.Id != request.TaskId)
            .OrderBy(t => t.OrderIndex)
            .ToListAsync(cancellationToken);

        // Insert at the new position
        for (int i = 0; i < tasksInNewColumn.Count; i++)
        {
            if (i >= request.NewOrderIndex)
            {
                tasksInNewColumn[i].OrderIndex = i + 1;
            }
        }

        task.OrderIndex = request.NewOrderIndex;
        task.UpdatedAt = DateTime.UtcNow;

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
            task.Comments.Select(c => new TaskCommentDto(c.Id, c.UserId, c.Content, c.CreatedAt)).ToList(),
            task.Attachments.Select(a => new TaskAttachmentDto(a.Id, a.FileName, a.FileSize, a.UploadedAt)).ToList());
    }
}
