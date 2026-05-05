using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.DTOs;
using TaskFlow.Application.Extensions;
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
        if (request.ProjectId.HasValue)
        {
            var projectExists = await _dbContext.Projects
                .AnyAsync(p => p.Id == request.ProjectId.Value && p.UserId == request.UserId, cancellationToken);
            if (!projectExists)
                throw new KeyNotFoundException("Project not found");
        }

        if (request.ParentTaskId.HasValue)
        {
            var parentExists = await _dbContext.Tasks
                .AnyAsync(t => t.Id == request.ParentTaskId.Value && t.UserId == request.UserId, cancellationToken);
            if (!parentExists)
                throw new KeyNotFoundException("Parent task not found");
        }

        var statusValue = request.Status ?? 0;
        if (!Enum.IsDefined(typeof(TaskStatus), statusValue))
            throw new ArgumentException($"Invalid status value: {statusValue}");
        var targetStatus = (TaskStatus)statusValue;

        TaskItem taskItem;

        if (_dbContext.Database.IsRelational())
        {
            // Postgres advisory lock serializes OrderIndex assignment per (user, status, parent).
            var strategy = _dbContext.Database.CreateExecutionStrategy();
            taskItem = await strategy.ExecuteAsync(async () =>
            {
                _dbContext.ChangeTracker.Clear();
                await using var tx = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

                var lockKey = request.ParentTaskId.HasValue
                    ? $"taskorder:{request.UserId}:parent:{request.ParentTaskId.Value}"
                    : $"taskorder:{request.UserId}:{(int)targetStatus}:root";
                await _dbContext.Database.ExecuteSqlRawAsync(
                    "SELECT pg_advisory_xact_lock(hashtext({0}))",
                    new object[] { lockKey },
                    cancellationToken);

                var item = await BuildAndSaveTaskAsync(request, targetStatus, cancellationToken);
                await tx.CommitAsync(cancellationToken);
                return item;
            });
        }
        else
        {
            taskItem = await BuildAndSaveTaskAsync(request, targetStatus, cancellationToken);
        }

        await _dbContext.Entry(taskItem).Collection(t => t.TaskTags).Query()
            .Include(tt => tt.Tag).LoadAsync(cancellationToken);

        var dto = taskItem.ToDto();
        await _tasksHub.Clients.User(request.UserId).SendAsync("TaskCreated", dto, cancellationToken);
        return dto;
    }

    private async Task<TaskItem> BuildAndSaveTaskAsync(CreateTaskCommand request, TaskStatus targetStatus, CancellationToken cancellationToken)
    {
        var orderIndex = request.ParentTaskId.HasValue
            ? await GetNextOrderIndex(request.UserId, targetStatus, request.ParentTaskId, cancellationToken)
            : await GetNextOrderIndex(request.UserId, targetStatus, null, cancellationToken);

        var item = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            Priority = (TaskPriority)request.Priority,
            Status = targetStatus,
            Deadline = request.Deadline.HasValue ? DateTime.SpecifyKind(request.Deadline.Value, DateTimeKind.Utc) : null,
            OrderIndex = orderIndex,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            UserId = request.UserId,
            ProjectId = request.ProjectId,
            ParentTaskId = request.ParentTaskId
        };

        _dbContext.Tasks.Add(item);

        if (request.TagIds?.Count > 0)
        {
            var tags = await _dbContext.Tags
                .Where(t => request.TagIds.Contains(t.Id) && t.UserId == request.UserId)
                .ToListAsync(cancellationToken);

            foreach (var tag in tags)
                item.TaskTags.Add(new TaskTag { TaskId = item.Id, TagId = tag.Id });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return item;
    }

    private async Task<int> GetNextOrderIndex(string userId, TaskStatus status, Guid? parentTaskId, CancellationToken cancellationToken)
    {
        var query = _dbContext.Tasks.Where(t => t.UserId == userId);
        query = parentTaskId.HasValue
            ? query.Where(t => t.ParentTaskId == parentTaskId.Value)
            : query.Where(t => t.ParentTaskId == null && t.Status == status);

        var maxOrder = await query.MaxAsync(t => (int?)t.OrderIndex, cancellationToken) ?? 0;
        return maxOrder + 1;
    }
}
