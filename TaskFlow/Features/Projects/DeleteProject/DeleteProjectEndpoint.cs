using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace TaskFlow.Features.Projects.DeleteProject;

[ApiController]
[Route("api/projects")]
[Authorize]
public class DeleteProjectEndpoint : ControllerBase
{
    private readonly IMediator _mediator;

    public DeleteProjectEndpoint(IMediator mediator) => _mediator = mediator;

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException();

        await _mediator.Send(new DeleteProjectCommand { ProjectId = id, UserId = userId }, cancellationToken);
        return NoContent();
    }
}
