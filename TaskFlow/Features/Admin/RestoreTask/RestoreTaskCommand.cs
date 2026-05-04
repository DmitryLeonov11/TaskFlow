using MediatR;
using TaskFlow.Application.DTOs;

namespace TaskFlow.Features.Admin.RestoreTask;

public class RestoreTaskCommand : IRequest<TaskItemDto>
{
    public Guid TaskId { get; set; }
}
