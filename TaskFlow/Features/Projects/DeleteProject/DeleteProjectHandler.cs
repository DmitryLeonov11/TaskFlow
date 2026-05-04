using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.Features.Projects.DeleteProject;

public class DeleteProjectHandler : IRequestHandler<DeleteProjectCommand, bool>
{
    private readonly ApplicationDbContext _dbContext;

    public DeleteProjectHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> Handle(DeleteProjectCommand request, CancellationToken cancellationToken)
    {
        var project = await _dbContext.Projects
            .FirstOrDefaultAsync(p => p.Id == request.ProjectId && p.UserId == request.UserId, cancellationToken)
            ?? throw new KeyNotFoundException($"Project {request.ProjectId} not found");

        // Detach tasks from project (SetNull via EF tracking, works with all providers)
        var projectTasks = await _dbContext.Tasks
            .Where(t => t.ProjectId == request.ProjectId)
            .ToListAsync(cancellationToken);

        foreach (var t in projectTasks)
            t.ProjectId = null;

        _dbContext.Projects.Remove(project);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}
