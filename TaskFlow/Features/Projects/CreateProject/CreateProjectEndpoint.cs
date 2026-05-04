using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskFlow.Application.DTOs;

namespace TaskFlow.Features.Projects.CreateProject;

[ApiController]
[Route("api/projects")]
[Authorize]
public class CreateProjectEndpoint : ControllerBase
{
    private readonly IMediator _mediator;

    public CreateProjectEndpoint(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    [ProducesResponseType(typeof(ProjectDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<ProjectDto>> Create(CreateProjectDto dto, CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException();

        var result = await _mediator.Send(new CreateProjectCommand
        {
            Name = dto.Name,
            Description = dto.Description,
            Color = dto.Color,
            UserId = userId
        }, cancellationToken);

        return CreatedAtAction(nameof(Create), new { id = result.Id }, result);
    }
}
