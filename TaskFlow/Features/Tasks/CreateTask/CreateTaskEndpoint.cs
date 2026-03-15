using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using System.Security.Claims;
using TaskFlow.Application.DTOs;
using TaskFlow.Features.Tasks.CreateTask;

namespace TaskFlow.Features.Tasks.CreateTask;

[ApiController]
[Route("api/tasks")]
[Authorize]
public class CreateTaskEndpoint : ControllerBase
{
    private readonly IMediator _mediator;

    public CreateTaskEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [ProducesResponseType(typeof(TaskItemDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TaskItemDto>> Create(CreateTaskDto dto, CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("User ID not found in token");

        var command = new CreateTaskCommand
        {
            Title = dto.Title,
            Description = dto.Description,
            Priority = dto.Priority,
            Deadline = dto.Deadline,
            TagIds = dto.TagIds,
            UserId = userId
        };

        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(Create), new { id = result.Id }, result);
    }
}
