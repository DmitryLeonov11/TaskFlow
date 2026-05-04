using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskFlow.Application.DTOs;

namespace TaskFlow.Features.Projects.UpdateProject;

[ApiController]
[Route("api/projects")]
[Authorize]
public class UpdateProjectEndpoint : ControllerBase
{
    private readonly IMediator _mediator;

    public UpdateProjectEndpoint(IMediator mediator) => _mediator = mediator;

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ProjectDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProjectDto>> Update(Guid id, UpdateProjectDto dto, CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException();

        var result = await _mediator.Send(new UpdateProjectCommand
        {
            ProjectId = id,
            Name = dto.Name,
            Description = dto.Description,
            Color = dto.Color,
            UserId = userId
        }, cancellationToken);

        return Ok(result);
    }
}
