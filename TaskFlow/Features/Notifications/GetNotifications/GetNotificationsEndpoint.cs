using Microsoft.AspNetCore.Mvc;

namespace TaskFlow.Features.Notifications.GetNotifications;

public static class GetNotificationsEndpoint
{
    public static void MapGetNotificationsEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/notifications", async (
            [FromQuery] int page,
            [FromQuery] int pageSize,
            [FromQuery] bool? isRead,
            IMediator mediator,
            HttpContext http,
            CancellationToken cancellationToken) =>
        {
            var userId = http.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                ?? throw new UnauthorizedAccessException("User not authenticated");

            var query = new GetNotificationsQuery
            {
                UserId = userId,
                Page = page > 0 ? page : 1,
                PageSize = pageSize > 0 ? pageSize : 20,
                IsRead = isRead
            };

            var result = await mediator.Send(query, cancellationToken);
            return Results.Ok(result);
        })
        .RequireAuthorization()
        .WithOpenApi(operation => new(operation)
        {
            Summary = "Get user notifications",
            Description = "Get paginated notifications for the current user"
        })
        .Produces<GetNotificationsResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized);
    }
}
