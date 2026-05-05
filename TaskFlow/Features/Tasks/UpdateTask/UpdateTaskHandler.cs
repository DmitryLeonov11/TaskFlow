using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.DTOs;
using TaskFlow.Application.Extensions;
using TaskFlow.Domain.Entities;
using TaskFlow.Hubs;
using TaskFlow.Infrastructure.Persistence;
using TaskStatus = TaskFlow.Domain.Entities.TaskStatus;

namespace TaskFlow.Features.Tasks.UpdateTask;

public class UpdateTaskHandler : IRequestHandler<UpdateTaskCommand, TaskItemDto>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IHubContext<TasksHub> _tasksHub;

    public UpdateTaskHandler(ApplicationDbContext dbContext, IHubContext<TasksHub> tasksHub)
    {
        _dbContext = dbContext;
        _tasksHub = tasksHub;
    }

    public async Task<TaskItemDto> Handle(UpdateTaskCommand request, CancellationToken cancellationToken)
    {
        var query = _dbContext.Tasks
            .Include(t => t.TaskTags).ThenInclude(tt => tt.Tag)
            .Include(t => t.Subtasks)
            .AsQueryable();

        var task = await query
            .FirstOrDefaultAsync(t => t.Id == request.TaskId && t.UserId == request.UserId, cancellationToken)
            ?? throw new KeyNotFoundException($"Task with id {request.TaskId} not found");

        if (request.Title.HasValue && !string.IsNullOrWhiteSpace(request.Title.Value))
            task.Title = request.Title.Value!;

        if (request.Description.HasValue)
            task.Description = request.Description.Value;

        if (request.Priority.HasValue)
        {
            var priorityValue = request.Priority.Value;
            if (!Enum.IsDefined(typeof(TaskPriority), priorityValue))
                throw new ArgumentException($"Invalid priority value: {priorityValue}");
            task.Priority = (TaskPriority)priorityValue;
        }

        if (request.Status.HasValue)
        {
            var statusValue = request.Status.Value;
            if (!Enum.IsDefined(typeof(TaskStatus), statusValue))
                throw new ArgumentException($"Invalid status value: {statusValue}");
            task.Status = (TaskStatus)statusValue;
        }

        if (request.Deadline.HasValue)
        {
            task.Deadline = request.Deadline.Value.HasValue
                ? DateTime.SpecifyKind(request.Deadline.Value.Value, DateTimeKind.Utc)
                : null;
        }

        task.UpdatedAt = DateTime.UtcNow;

        if (request.TagIds.HasValue)
        {
            _dbContext.TaskTags.RemoveRange(task.TaskTags);

            var ids = request.TagIds.Value;
            if (ids != null && ids.Count > 0)
            {
                var tags = await _dbContext.Tags
                    .Where(t => ids.Contains(t.Id) && t.UserId == request.UserId)
                    .ToListAsync(cancellationToken);

                foreach (var tag in tags)
                {
                    task.TaskTags.Add(new TaskTag { TaskId = task.Id, TagId = tag.Id });
                }
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        // Reload tags+attachments+comments for the response DTO so it is consistent
        await _dbContext.Entry(task).Collection(t => t.TaskTags).Query().Include(tt => tt.Tag).LoadAsync(cancellationToken);
        await _dbContext.Entry(task).Collection(t => t.Comments).LoadAsync(cancellationToken);
        await _dbContext.Entry(task).Collection(t => t.Attachments).LoadAsync(cancellationToken);

        var dto = task.ToDto();

        if (task.ParentTaskId == null)
            await _tasksHub.Clients.User(request.UserId).SendAsync("TaskUpdated", dto, cancellationToken);

        return dto;
    }
}
