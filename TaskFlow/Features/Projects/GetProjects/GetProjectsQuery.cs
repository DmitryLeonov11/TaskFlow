using MediatR;
using TaskFlow.Application.DTOs;

namespace TaskFlow.Features.Projects.GetProjects;

public class GetProjectsQuery : IRequest<IReadOnlyList<ProjectDto>>
{
    public required string UserId { get; set; }
}
