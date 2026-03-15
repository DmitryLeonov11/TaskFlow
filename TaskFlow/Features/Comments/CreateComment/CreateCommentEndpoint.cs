using MediatR;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.DTOs;

namespace TaskFlow.Features.Comments.CreateComment;

public static class CreateCommentEndpoint
{
    public static void MapCreateCommentEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/tasks/{taskId:guid}/comments", async (
            Guid taskId,
            CreateCommentDto dto,
            IMediator mediator,
            HttpContext http,
            CancellationToken cancellationToken) =>
        {
            var userId = http.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                ?? throw new UnauthorizedAccessException("User not authenticated");

            var command = new CreateCommentCommand
            {
                TaskId = taskId,
                Content = dto.Content,
                UserId = userId
            };

            var result = await mediator.Send(command, cancellationToken);
            return Results.Created($"/api/tasks/{taskId}/comments/{result.Id}", result);
        })
        .RequireAuthorization()
        .WithOpenApi(operation => new(operation)
        {
            Summary = "Add comment to task",
            Description = "Add a new comment to a specific task"
        })
        .Produces<TaskCommentDto>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound);
    }
}
