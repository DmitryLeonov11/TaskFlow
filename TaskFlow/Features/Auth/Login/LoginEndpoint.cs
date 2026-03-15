using MediatR;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.DTOs;

namespace TaskFlow.Features.Auth.Login;

public static class LoginEndpoint
{
    public static void MapLoginEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/login", async (
            LoginCommand command,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(command, cancellationToken);
            return Results.Ok(result);
        })
        .WithOpenApi(operation => new(operation)
        {
            Summary = "Login user",
            Description = "Authenticate user with email and password"
        })
        .AllowAnonymous()
        .Produces<AuthResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized);
    }
}
