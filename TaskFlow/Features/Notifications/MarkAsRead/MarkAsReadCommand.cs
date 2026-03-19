using MediatR;

namespace TaskFlow.Features.Notifications.MarkAsRead;

public class MarkAsReadCommand : IRequest<Unit>
{
    public Guid NotificationId { get; set; }
    public required string UserId { get; set; }
}