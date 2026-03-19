using MediatR;

namespace TaskFlow.Features.Tasks.DeleteTask;

public class DeleteTaskCommand : IRequest<bool>
{
    public Guid TaskId { get; set; }
    public required string UserId { get; set; }
}