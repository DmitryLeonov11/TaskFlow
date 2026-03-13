using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using System.Security.Claims;
using TaskFlow.Features.Tasks.DeleteTask;

namespace TaskFlow.Features.Tasks.DeleteTask;

[ApiController]
[Route("api/tasks")]
[Authorize]
public class DeleteTaskEndpoint : ControllerBase
{
    private readonly IMediator _mediator;

    public DeleteTaskEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("User ID not found in token");

        var command = new DeleteTaskCommand { TaskId = id, UserId = userId };

        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }
}
