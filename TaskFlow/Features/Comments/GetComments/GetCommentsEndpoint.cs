using Microsoft.AspNetCore.Mvc;

namespace TaskFlow.Features.Comments.GetComments;

public static class GetCommentsEndpoint
{
    public static void MapGetCommentsEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/tasks/{taskId:guid}/comments", async (
            Guid taskId,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var query = new GetCommentsQuery { TaskId = taskId };
            var result = await mediator.Send(query, cancellationToken);
            return Results.Ok(result);
        })
        .RequireAuthorization()
        .WithOpenApi(operation => new(operation)
        {
            Summary = "Get task comments",
            Description = "Get all comments for a specific task"
        })
        .Produces<IReadOnlyList<TaskCommentDto>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized);
    }
}
