using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.DTOs;

namespace TaskFlow.Features.Admin.RestoreTask;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class RestoreTaskEndpoint : ControllerBase
{
    private readonly IMediator _mediator;

    public RestoreTaskEndpoint(IMediator mediator) => _mediator = mediator;

    [HttpPost("tasks/{id:guid}/restore")]
    [ProducesResponseType(typeof(TaskItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskItemDto>> Restore(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new RestoreTaskCommand { TaskId = id }, cancellationToken);
        return Ok(result);
    }
}
