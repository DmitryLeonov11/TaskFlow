using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskFlow.Features.Tasks.GetTasks;

namespace TaskFlow.Features.Tasks.GetTasks;

[ApiController]
[Route("api/tasks")]
[Authorize]
public class GetTasksEndpoint : ControllerBase
{
    private readonly IMediator _mediator;

    public GetTasksEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(GetTasksResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<GetTasksResponse>> GetTasks(
        [FromQuery] int? status = null,
        [FromQuery] string? searchTerm = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("User ID not found in token");

        var query = new GetTasksQuery
        {
            UserId = userId,
            Status = status,
            SearchTerm = searchTerm,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }
}