using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using System.Security.Claims;
using TaskFlow.Application.DTOs;
using TaskFlow.Features.Tasks.UpdateTask;

namespace TaskFlow.Features.Tasks.UpdateTask;

[ApiController]
[Route("api/tasks")]
[Authorize]
public class UpdateTaskEndpoint : ControllerBase
{
    private readonly IMediator _mediator;

    public UpdateTaskEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(TaskItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskItemDto>> Update(Guid id, UpdateTaskDto dto, CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("User ID not found in token");

        var command = new UpdateTaskCommand
        {
            TaskId = id,
            Title = dto.Title,
            Description = dto.Description,
            Priority = dto.Priority,
            Status = dto.Status,
            Deadline = dto.Deadline,
            TagIds = dto.TagIds,
            UserId = userId
        };

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }
}
