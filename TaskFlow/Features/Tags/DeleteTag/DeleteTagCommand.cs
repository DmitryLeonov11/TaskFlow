using MediatR;

namespace TaskFlow.Features.Tags.DeleteTag;

public class DeleteTagCommand : IRequest<bool>
{
    public required Guid TagId { get; set; }
    public required string UserId { get; set; }
}