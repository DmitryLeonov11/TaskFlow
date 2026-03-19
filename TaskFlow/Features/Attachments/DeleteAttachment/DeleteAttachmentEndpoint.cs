using MediatR;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.DTOs;

namespace TaskFlow.Features.Attachments.DeleteAttachment;

public static class DeleteAttachmentEndpoint
{
    public static void MapDeleteAttachmentEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/tasks/attachments/{attachmentId:guid}", async (
            Guid attachmentId,
            IMediator mediator,
            HttpContext http,
            CancellationToken cancellationToken) =>
        {
            var userId = http.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                ?? throw new UnauthorizedAccessException("User not authenticated");

            var command = new DeleteAttachmentCommand
            {
                AttachmentId = attachmentId,
                UserId = userId
            };

            await mediator.Send(command, cancellationToken);
            return Results.NoContent();
        })
        .RequireAuthorization()
        .WithOpenApi(operation => new(operation)
        {
            Summary = "Delete attachment",
            Description = "Delete a specific attachment from a task"
        })
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound);
    }
}