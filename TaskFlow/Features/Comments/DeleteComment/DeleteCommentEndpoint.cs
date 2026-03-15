using MediatR;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.DTOs;

namespace TaskFlow.Features.Comments.DeleteComment;

public static class DeleteCommentEndpoint
{
    public static void MapDeleteCommentEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/tasks/comments/{commentId:guid}", async (
            Guid commentId,
            IMediator mediator,
            HttpContext http,
            CancellationToken cancellationToken) =>
        {
            var userId = http.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                ?? throw new UnauthorizedAccessException("User not authenticated");

            var command = new DeleteCommentCommand
            {
                CommentId = commentId,
                UserId = userId
            };

            await mediator.Send(command, cancellationToken);
            return Results.NoContent();
        })
        .RequireAuthorization()
        .WithOpenApi(operation => new(operation)
        {
            Summary = "Delete comment",
            Description = "Delete a specific comment from a task"
        })
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound);
    }
}
