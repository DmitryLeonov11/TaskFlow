using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.DTOs;
using TaskFlow.Domain.Entities;
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
            .Where(t => t.UserId == request.UserId);

        // Filter by status
        if (request.Status.HasValue)
        {
            query = query.Where(t => (int)t.Status == request.Status.Value);
        }

        // Filter by tags
        if (request.TagIds?.Count > 0)
        {
            query = query.Where(t => t.TaskTags.Any(tt => request.TagIds.Contains(tt.TagId)));
        }

        // Search
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

        var dtos = tasks.Select(t => MapToDto(t)).ToList();

        return new GetTasksResponse(dtos, totalCount, request.PageNumber, request.PageSize);
    }

    private static TaskItemDto MapToDto(TaskItem task)
    {
        return new TaskItemDto(
            task.Id,
            task.Title,
            task.Description,
            (int)task.Priority,
            (int)task.Status,
            task.Deadline,
            task.OrderIndex,
            task.CreatedAt,
            task.UpdatedAt,
            task.TaskTags.Select(tt => new TagDto(tt.Tag.Id, tt.Tag.Name, tt.Tag.Color)).ToList(),
            task.Comments.Select(c => new TaskCommentDto(c.Id, c.UserId, c.Content, c.CreatedAt)).ToList(),
            task.Attachments.Select(a => new TaskAttachmentDto(a.Id, a.FileName, a.FileSize, a.UploadedAt)).ToList());
    }
}
