using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.DTOs;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.Features.Comments.GetComments;

public class GetCommentsHandler : IRequestHandler<GetCommentsQuery, IReadOnlyList<TaskCommentDto>>
{
    private readonly ApplicationDbContext _dbContext;

    public GetCommentsHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<TaskCommentDto>> Handle(GetCommentsQuery request, CancellationToken cancellationToken)
    {
        var comments = await _dbContext.TaskComments
            .Where(c => c.TaskId == request.TaskId)
            .OrderBy(c => c.CreatedAt)
            .Select(c => new TaskCommentDto(
                c.Id,
                c.UserId,
                c.Content,
                c.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        return comments;
    }
}