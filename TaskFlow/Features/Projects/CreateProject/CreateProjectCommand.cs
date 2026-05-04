using MediatR;
using TaskFlow.Application.DTOs;

namespace TaskFlow.Features.Projects.CreateProject;

public class CreateProjectCommand : IRequest<ProjectDto>
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public string? Color { get; set; }
    public required string UserId { get; set; }
}
