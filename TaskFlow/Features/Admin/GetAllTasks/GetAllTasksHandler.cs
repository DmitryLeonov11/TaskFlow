using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.DTOs;
using TaskFlow.Application.Extensions;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.Features.Admin.GetAllTasks;

public class GetAllTasksHandler : IRequestHandler<GetAllTasksQuery, GetTasksResponse>
{
    private readonly ApplicationDbContext _dbContext;

    public GetAllTasksHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GetTasksResponse> Handle(GetAllTasksQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.Tasks
            .IgnoreQueryFilters()
            .Include(t => t.TaskTags)
            .ThenInclude(tt => tt.Tag)
            .Include(t => t.Comments)
            .Include(t => t.Attachments)
            .Include(t => t.Subtasks)
            .AsQueryable();

        if (!request.IncludeDeleted)
            query = query.Where(t => !t.IsDeleted);

        var total = await query.CountAsync(cancellationToken);
        var tasks = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new GetTasksResponse(
            tasks.Select(t => t.ToDto()).ToList(),
            total,
            request.PageNumber,
            request.PageSize);
    }
}
