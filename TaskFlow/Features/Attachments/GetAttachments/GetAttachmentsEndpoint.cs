using MediatR;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.DTOs;

namespace TaskFlow.Features.Attachments.GetAttachments;

public static class GetAttachmentsEndpoint
{
    public static void MapGetAttachmentsEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/tasks/{taskId:guid}/attachments", async (
            Guid taskId,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var query = new GetAttachmentsQuery { TaskId = taskId };
            var result = await mediator.Send(query, cancellationToken);
            return Results.Ok(result);
        })
        .RequireAuthorization()
        .WithOpenApi(operation => new(operation)
        {
            Summary = "Get task attachments",
            Description = "Get all attachments for a specific task"
        })
        .Produces<IReadOnlyList<TaskAttachmentDto>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized);
    }
}