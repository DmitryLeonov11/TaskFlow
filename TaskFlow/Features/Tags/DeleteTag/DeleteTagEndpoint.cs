using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using System.Security.Claims;

namespace TaskFlow.Features.Tags.DeleteTag;

[ApiController]
[Route("api/tags")]
[Authorize]
public class DeleteTagEndpoint : ControllerBase
{
    private readonly IMediator _mediator;

    public DeleteTagEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("User ID not found in token");

        var command = new DeleteTagCommand { TagId = id, UserId = userId };
        var result = await _mediator.Send(command, cancellationToken);

        return result ? NoContent() : NotFound();
    }
}
