using MediatR;
using TaskFlow.Application.DTOs;

namespace TaskFlow.Features.Tasks.MoveTask;

public class MoveTaskCommand : IRequest<TaskItemDto>
{
    public Guid TaskId { get; set; }
    public int NewStatus { get; set; }
    public int NewOrderIndex { get; set; }
    public required string UserId { get; set; }
}
