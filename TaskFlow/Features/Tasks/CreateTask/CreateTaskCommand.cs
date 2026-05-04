using MediatR;
using TaskFlow.Application.DTOs;

namespace TaskFlow.Features.Tasks.CreateTask;

public class CreateTaskCommand : IRequest<TaskItemDto>
{
    public required string Title { get; set; }
    public string? Description { get; set; }
    public int Priority { get; set; }
    public int? Status { get; set; }
    public DateTime? Deadline { get; set; }
    public IReadOnlyList<Guid>? TagIds { get; set; }
    public Guid? ProjectId { get; set; }
    public Guid? ParentTaskId { get; set; }
    public required string UserId { get; set; }
}