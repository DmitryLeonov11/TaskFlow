using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.DTOs;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.Features.Notifications.GetNotifications;

public class GetNotificationsHandler : IRequestHandler<GetNotificationsQuery, GetNotificationsResponse>
{
    private readonly ApplicationDbContext _dbContext;

    public GetNotificationsHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GetNotificationsResponse> Handle(GetNotificationsQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.Notifications
            .Where(n => n.UserId == request.UserId);

        if (request.IsRead.HasValue)
        {
            query = query.Where(n => n.IsRead == request.IsRead.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var notifications = await query
            .OrderByDescending(n => n.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(n => new NotificationDto(
                n.Id,
                n.Type.ToString(),
                n.Message,
                n.RelatedTaskId,
                n.IsRead,
                n.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        var unreadCount = await _dbContext.Notifications
            .Where(n => n.UserId == request.UserId && !n.IsRead)
            .CountAsync(cancellationToken);

        return new GetNotificationsResponse(notifications, totalCount, unreadCount);
    }
}