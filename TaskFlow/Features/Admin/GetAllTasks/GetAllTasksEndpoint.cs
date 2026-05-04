using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.DTOs;

namespace TaskFlow.Features.Admin.GetAllTasks;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class GetAllTasksEndpoint : ControllerBase
{
    private readonly IMediator _mediator;

    public GetAllTasksEndpoint(IMediator mediator) => _mediator = mediator;

    [HttpGet("tasks")]
    [ProducesResponseType(typeof(GetTasksResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<GetTasksResponse>> GetTasks(
        [FromQuery] bool includeDeleted = true,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetAllTasksQuery
        {
            IncludeDeleted = includeDeleted,
            PageNumber = pageNumber,
            PageSize = pageSize
        }, cancellationToken);

        return Ok(result);
    }
}
