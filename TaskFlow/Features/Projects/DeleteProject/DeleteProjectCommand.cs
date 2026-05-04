using MediatR;

namespace TaskFlow.Features.Projects.DeleteProject;

public class DeleteProjectCommand : IRequest<bool>
{
    public Guid ProjectId { get; set; }
    public required string UserId { get; set; }
}
