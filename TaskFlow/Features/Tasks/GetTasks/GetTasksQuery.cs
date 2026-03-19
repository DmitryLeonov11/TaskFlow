using MediatR;
using TaskFlow.Application.DTOs;

namespace TaskFlow.Features.Tasks.GetTasks;

public class GetTasksQuery : IRequest<GetTasksResponse>
{
    public required string UserId { get; set; }
    public int? Status { get; set; }
    public IReadOnlyList<Guid>? TagIds { get; set; }
    public string? SearchTerm { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

public record GetTasksResponse(
    IReadOnlyList<TaskItemDto> Tasks,
    int TotalCount,
    int PageNumber,
    int PageSize);