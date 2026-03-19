using MediatR;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.DTOs;

namespace TaskFlow.Features.Auth.ResetPassword;

public static class ResetPasswordEndpoint
{
    public static void MapResetPasswordEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/request-password-reset", async (
            RequestPasswordResetCommand command,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            await mediator.Send(command, cancellationToken);
            return Results.NoContent();
        })
        .AllowAnonymous()
        .WithOpenApi(operation => new(operation)
        {
            Summary = "Request password reset",
            Description = "Request a password reset link for the specified email"
        })
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest);

        app.MapPost("/api/auth/reset-password", async (
            ResetPasswordCommand command,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            await mediator.Send(command, cancellationToken);
            return Results.NoContent();
        })
        .AllowAnonymous()
        .WithOpenApi(operation => new(operation)
        {
            Summary = "Reset password",
            Description = "Reset password with token from email"
        })
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest);
    }
}