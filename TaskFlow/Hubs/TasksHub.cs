using Microsoft.AspNetCore.SignalR;

namespace TaskFlow.Hubs;

public class TasksHub : Hub
{
    public async Task SubscribeToTask(Guid taskId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"task-{taskId}");
    }

    public async Task UnsubscribeFromTask(Guid taskId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"task-{taskId}");
    }

    public async Task TaskCreated(TaskCreatedEvent taskEvent)
    {
        await Clients.User(taskEvent.UserId).SendAsync("TaskCreated", taskEvent);
    }

    public async Task TaskUpdated(TaskUpdatedEvent taskEvent)
    {
        await Clients.User(taskEvent.UserId).SendAsync("TaskUpdated", taskEvent);
    }

    public async Task TaskMoved(TaskMovedEvent taskEvent)
    {
        await Clients.User(taskEvent.UserId).SendAsync("TaskMoved", taskEvent);
    }
}

public record TaskCreatedEvent(
    Guid TaskId,
    string UserId,
    string Title,
    int Priority,
    int Status,
    DateTime CreatedAt
);

public record TaskUpdatedEvent(
    Guid TaskId,
    string UserId,
    string Title,
    int Priority,
    int Status,
    DateTime UpdatedAt
);

public record TaskMovedEvent(
    Guid TaskId,
    string UserId,
    int OldStatus,
    int NewStatus,
    int NewOrderIndex,
    DateTime MovedAt
);