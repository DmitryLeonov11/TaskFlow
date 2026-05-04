using MediatR;
using TaskFlow.Application.DTOs;

namespace TaskFlow.Features.Admin.GetAllTasks;

public class GetAllTasksQuery : IRequest<GetTasksResponse>
{
    public bool IncludeDeleted { get; set; } = true;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

public record GetTasksResponse(
    IReadOnlyList<TaskItemDto> Tasks,
    int TotalCount,
    int PageNumber,
    int PageSize);
