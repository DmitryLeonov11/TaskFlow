using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.DTOs;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.Features.Projects.UpdateProject;

public class UpdateProjectHandler : IRequestHandler<UpdateProjectCommand, ProjectDto>
{
    private readonly ApplicationDbContext _dbContext;

    public UpdateProjectHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ProjectDto> Handle(UpdateProjectCommand request, CancellationToken cancellationToken)
    {
        var project = await _dbContext.Projects
            .FirstOrDefaultAsync(p => p.Id == request.ProjectId && p.UserId == request.UserId, cancellationToken)
            ?? throw new KeyNotFoundException($"Project {request.ProjectId} not found");

        if (!string.IsNullOrWhiteSpace(request.Name))
            project.Name = request.Name;

        if (request.Description != null)
            project.Description = request.Description;

        if (request.Color != null)
            project.Color = request.Color;

        project.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);

        var taskCount = await _dbContext.Tasks.CountAsync(t => t.ProjectId == project.Id, cancellationToken);

        return new ProjectDto(project.Id, project.Name, project.Description, project.Color, project.CreatedAt, taskCount);
    }
}
