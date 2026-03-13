using Microsoft.AspNetCore.SignalR;

namespace TaskFlow.Hubs;

public class NotificationsHub : Hub
{
    public async Task SubscribeToNotifications()
    {
        var userId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userId != null)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");
        }
    }

    public async Task SendNotification(NotificationEvent notificationEvent)
    {
        await Clients.User(notificationEvent.UserId).SendAsync("NotificationReceived", notificationEvent);
    }
}

public record NotificationEvent(
    Guid NotificationId,
    string UserId,
    string Type,
    string Message,
    Guid? RelatedTaskId,
    DateTime CreatedAt
);
