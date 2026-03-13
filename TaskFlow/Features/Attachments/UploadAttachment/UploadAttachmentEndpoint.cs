using Microsoft.AspNetCore.Mvc;

namespace TaskFlow.Features.Attachments.UploadAttachment;

public static class UploadAttachmentEndpoint
{
    public static void MapUploadAttachmentEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/tasks/{taskId:guid}/attachments", async (
            Guid taskId,
            IFormFile file,
            IMediator mediator,
            HttpContext http,
            CancellationToken cancellationToken) =>
        {
            var userId = http.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                ?? throw new UnauthorizedAccessException("User not authenticated");

            if (file == null || file.Length == 0)
            {
                return Results.BadRequest("No file uploaded");
            }

            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream, cancellationToken);
            var fileContent = memoryStream.ToArray();

            var command = new UploadAttachmentCommand
            {
                TaskId = taskId,
                FileName = file.FileName,
                ContentType = file.ContentType,
                FileSize = file.Length,
                FileContent = fileContent,
                UserId = userId,
                UploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "uploads")
            };

            var result = await mediator.Send(command, cancellationToken);
            return Results.Created($"/api/tasks/{taskId}/attachments/{result.Id}", result);
        })
        .RequireAuthorization()
        .DisableFormValueReaderLimit()
        .WithOpenApi(operation => new(operation)
        {
            Summary = "Upload attachment to task",
            Description = "Upload a file attachment to a specific task (max 10 MB)"
        })
        .Produces<TaskAttachmentDto>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound);
    }
}
