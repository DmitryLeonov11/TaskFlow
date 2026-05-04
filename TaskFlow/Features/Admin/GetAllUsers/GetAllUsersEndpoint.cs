using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.DTOs;

namespace TaskFlow.Features.Admin.GetAllUsers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class GetAllUsersEndpoint : ControllerBase
{
    private readonly IMediator _mediator;

    public GetAllUsersEndpoint(IMediator mediator) => _mediator = mediator;

    [HttpGet("users")]
    [ProducesResponseType(typeof(IReadOnlyList<AdminUserDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AdminUserDto>>> GetUsers(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetAllUsersQuery(), cancellationToken);
        return Ok(result);
    }
}
