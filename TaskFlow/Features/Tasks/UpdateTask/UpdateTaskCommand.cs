using MediatR;
using TaskFlow.Application.Common;
using TaskFlow.Application.DTOs;

namespace TaskFlow.Features.Tasks.UpdateTask;

public class UpdateTaskCommand : IRequest<TaskItemDto>
{
    public Guid TaskId { get; set; }
    public Optional<string> Title { get; set; }
    public Optional<string?> Description { get; set; }
    public Optional<int> Priority { get; set; }
    public Optional<int> Status { get; set; }
    public Optional<DateTime?> Deadline { get; set; }
    public Optional<IReadOnlyList<Guid>?> TagIds { get; set; }
    public required string UserId { get; set; }
}
