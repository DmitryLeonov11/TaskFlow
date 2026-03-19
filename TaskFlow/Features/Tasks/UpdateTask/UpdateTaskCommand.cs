using MediatR;
using TaskFlow.Application.DTOs;

namespace TaskFlow.Features.Tasks.UpdateTask;

public class UpdateTaskCommand : IRequest<TaskItemDto>
{
    public Guid TaskId { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public int? Priority { get; set; }
    public int? Status { get; set; }
    public DateTime? Deadline { get; set; }
    public IReadOnlyList<Guid>? TagIds { get; set; }
    public required string UserId { get; set; }
}