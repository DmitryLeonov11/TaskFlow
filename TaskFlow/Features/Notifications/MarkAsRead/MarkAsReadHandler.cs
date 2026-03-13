using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.Features.Notifications.MarkAsRead;

public class MarkAsReadHandler : IRequestHandler<MarkAsReadCommand, Unit>
{
    private readonly ApplicationDbContext _dbContext;

    public MarkAsReadHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Unit> Handle(MarkAsReadCommand request, CancellationToken cancellationToken)
    {
        var notification = await _dbContext.Notifications
            .FirstOrDefaultAsync(n => n.Id == request.NotificationId, cancellationToken);

        if (notification == null)
        {
            throw new InvalidOperationException("Notification not found");
        }

        if (notification.UserId != request.UserId)
        {
            throw new UnauthorizedAccessException("You can only mark your own notifications as read");
        }

        notification.IsRead = true;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
