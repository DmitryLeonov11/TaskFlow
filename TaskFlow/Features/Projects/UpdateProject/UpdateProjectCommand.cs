using MediatR;
using TaskFlow.Application.DTOs;

namespace TaskFlow.Features.Projects.UpdateProject;

public class UpdateProjectCommand : IRequest<ProjectDto>
{
    public Guid ProjectId { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Color { get; set; }
    public required string UserId { get; set; }
}
