using MediatR;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.DTOs;

namespace TaskFlow.Features.Notifications.MarkAsRead;

public static class MarkAsReadEndpoint
{
    public static void MapMarkAsReadEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPut("/api/notifications/{notificationId:guid}/mark-as-read", async (
            Guid notificationId,
            IMediator mediator,
            HttpContext http,
            CancellationToken cancellationToken) =>
        {
            var userId = http.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                ?? throw new UnauthorizedAccessException("User not authenticated");

            var command = new MarkAsReadCommand
            {
                NotificationId = notificationId,
                UserId = userId
            };

            await mediator.Send(command, cancellationToken);
            return Results.NoContent();
        })
        .RequireAuthorization()
        .WithOpenApi(operation => new(operation)
        {
            Summary = "Mark notification as read",
            Description = "Mark a specific notification as read"
        })
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound);
    }
}
