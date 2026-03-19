using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskFlow.Application.DTOs;
using TaskFlow.Features.Tasks.MoveTask;

namespace TaskFlow.Features.Tasks.MoveTask;

[ApiController]
[Route("api/tasks")]
[Authorize]
public class MoveTaskEndpoint : ControllerBase
{
    private readonly IMediator _mediator;

    public MoveTaskEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPut("{id}/move")]
    [ProducesResponseType(typeof(TaskItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskItemDto>> Move(Guid id, MoveTaskDto dto, CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("User ID not found in token");

        var command = new MoveTaskCommand
        {
            TaskId = id,
            NewStatus = dto.NewStatus,
            NewOrderIndex = dto.NewOrderIndex,
            UserId = userId
        };

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }
}