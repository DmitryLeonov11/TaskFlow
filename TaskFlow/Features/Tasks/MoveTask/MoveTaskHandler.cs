using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.DTOs;
using TaskFlow.Application.Extensions;
using TaskFlow.Domain.Entities;
using TaskFlow.Hubs;
using TaskFlow.Infrastructure.Persistence;
using TaskStatus = TaskFlow.Domain.Entities.TaskStatus;

namespace TaskFlow.Features.Tasks.MoveTask;

public class MoveTaskHandler : IRequestHandler<MoveTaskCommand, TaskItemDto>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IHubContext<TasksHub> _tasksHub;

    public MoveTaskHandler(ApplicationDbContext dbContext, IHubContext<TasksHub> tasksHub)
    {
        _dbContext = dbContext;
        _tasksHub = tasksHub;
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

        // Normalize indices and insert at new position
        for (int i = 0; i < tasksInNewColumn.Count; i++)
        {
            tasksInNewColumn[i].OrderIndex = i < request.NewOrderIndex ? i : i + 1;
        }

        task.OrderIndex = Math.Min(request.NewOrderIndex, tasksInNewColumn.Count);
        task.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        var dto = task.ToDto();

        await _tasksHub.Clients.User(request.UserId).SendAsync("TaskMoved", new
        {
            taskId = task.Id,
            userId = request.UserId,
            oldStatus = (int)oldStatus,
            newStatus = (int)newStatus,
            newOrderIndex = task.OrderIndex,
            movedAt = DateTime.UtcNow
        }, cancellationToken);

        return dto;
    }

}