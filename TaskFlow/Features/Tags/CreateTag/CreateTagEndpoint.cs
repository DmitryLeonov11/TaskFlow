using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using System.Security.Claims;
using TaskFlow.Application.DTOs;
using TaskFlow.Features.Tags.CreateTag;

namespace TaskFlow.Features.Tags.CreateTag;

[ApiController]
[Route("api/tags")]
[Authorize]
public class CreateTagEndpoint : ControllerBase
{
    private readonly IMediator _mediator;

    public CreateTagEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [ProducesResponseType(typeof(TagDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TagDto>> Create(CreateTagDto dto, CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("User ID not found in token");

        var command = new CreateTagCommand
        {
            Name = dto.Name,
            Color = dto.Color,
            UserId = userId
        };

        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(Create), new { id = result.Id }, result);
    }
}
