using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskFlow.Application.DTOs;

namespace TaskFlow.Features.Projects.GetProjects;

[ApiController]
[Route("api/projects")]
[Authorize]
public class GetProjectsEndpoint : ControllerBase
{
    private readonly IMediator _mediator;

    public GetProjectsEndpoint(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ProjectDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProjectDto>>> GetAll(CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException();

        var result = await _mediator.Send(new GetProjectsQuery { UserId = userId }, cancellationToken);
        return Ok(result);
    }
}
