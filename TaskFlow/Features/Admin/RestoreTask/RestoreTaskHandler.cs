using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.DTOs;
using TaskFlow.Application.Extensions;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.Features.Admin.RestoreTask;

public class RestoreTaskHandler : IRequestHandler<RestoreTaskCommand, TaskItemDto>
{
    private readonly ApplicationDbContext _dbContext;

    public RestoreTaskHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TaskItemDto> Handle(RestoreTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await _dbContext.Tasks
            .IgnoreQueryFilters()
            .Include(t => t.TaskTags)
            .ThenInclude(tt => tt.Tag)
            .Include(t => t.Comments)
            .Include(t => t.Attachments)
            .Include(t => t.Subtasks)
            .FirstOrDefaultAsync(t => t.Id == request.TaskId, cancellationToken)
            ?? throw new KeyNotFoundException($"Task {request.TaskId} not found");

        task.IsDeleted = false;
        task.DeletedAt = null;
        task.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return task.ToDto();
    }
}
