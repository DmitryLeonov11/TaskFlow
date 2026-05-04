using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.DTOs;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.Features.Projects.GetProjects;

public class GetProjectsHandler : IRequestHandler<GetProjectsQuery, IReadOnlyList<ProjectDto>>
{
    private readonly ApplicationDbContext _dbContext;

    public GetProjectsHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ProjectDto>> Handle(GetProjectsQuery request, CancellationToken cancellationToken)
    {
        var projects = await _dbContext.Projects
            .Where(p => p.UserId == request.UserId)
            .OrderBy(p => p.CreatedAt)
            .Select(p => new ProjectDto(
                p.Id,
                p.Name,
                p.Description,
                p.Color,
                p.CreatedAt,
                p.Tasks.Count(t => !t.IsDeleted)))
            .ToListAsync(cancellationToken);

        return projects;
    }
}
