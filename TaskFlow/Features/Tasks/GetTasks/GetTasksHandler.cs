using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.DTOs;
using TaskFlow.Application.Extensions;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.Features.Tasks.GetTasks;

public class GetTasksHandler : IRequestHandler<GetTasksQuery, GetTasksResponse>
{
    private readonly ApplicationDbContext _dbContext;

    public GetTasksHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GetTasksResponse> Handle(GetTasksQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.Tasks
            .Include(t => t.TaskTags)
            .ThenInclude(tt => tt.Tag)
            .Include(t => t.Comments)
            .Include(t => t.Attachments)
            .Include(t => t.Subtasks)
            .Where(t => t.UserId == request.UserId && t.ParentTaskId == null);

        if (request.ProjectId.HasValue)
            query = query.Where(t => t.ProjectId == request.ProjectId.Value);

        if (request.Status.HasValue)
            query = query.Where(t => (int)t.Status == request.Status.Value);

        if (request.TagIds?.Count > 0)
            query = query.Where(t => t.TaskTags.Any(tt => request.TagIds.Contains(tt.TagId)));

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchLower = request.SearchTerm.ToLower();
            query = query.Where(t => t.Title.ToLower().Contains(searchLower) ||
                                     (t.Description != null && t.Description.ToLower().Contains(searchLower)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var tasks = await query
            .OrderBy(t => t.Status)
            .ThenBy(t => t.OrderIndex)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var dtos = tasks.Select(t => t.ToDto()).ToList();

        return new GetTasksResponse(dtos, totalCount, request.PageNumber, request.PageSize);
    }

}