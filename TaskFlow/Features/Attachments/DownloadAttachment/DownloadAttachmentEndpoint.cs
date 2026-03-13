using Microsoft.AspNetCore.Mvc;

namespace TaskFlow.Features.Attachments.DownloadAttachment;

public static class DownloadAttachmentEndpoint
{
    public static void MapDownloadAttachmentEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/tasks/attachments/{attachmentId:guid}/download", async (
            Guid attachmentId,
            IMediator mediator,
            HttpContext http,
            CancellationToken cancellationToken) =>
        {
            var userId = http.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                ?? throw new UnauthorizedAccessException("User not authenticated");

            var query = new DownloadAttachmentQuery
            {
                AttachmentId = attachmentId,
                UserId = userId
            };

            var (content, contentType, fileName) = await mediator.Send(query, cancellationToken);
            return Results.File(content, contentType, fileName);
        })
        .RequireAuthorization()
        .WithOpenApi(operation => new(operation)
        {
            Summary = "Download attachment",
            Description = "Download a specific attachment file"
        })
        .Produces<FileContentResult>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound);
    }
}
