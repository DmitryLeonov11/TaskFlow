using MediatR;
using TaskFlow.Application.DTOs;

namespace TaskFlow.Features.Notifications.GetNotifications;

public class GetNotificationsQuery : IRequest<GetNotificationsResponse>
{
    public required string UserId { get; set; }
    public bool? IsRead { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}