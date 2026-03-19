using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.DTOs;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.Features.Tags.GetTags;

public class GetTagsHandler : IRequestHandler<GetTagsQuery, IReadOnlyList<TagDto>>
{
    private readonly ApplicationDbContext _dbContext;

    public GetTagsHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<TagDto>> Handle(GetTagsQuery request, CancellationToken cancellationToken)
    {
        var tags = await _dbContext.Tags
            .Where(t => t.UserId == request.UserId)
            .OrderBy(t => t.Name)
            .Select(t => new TagDto(t.Id, t.Name, t.Color))
            .ToListAsync(cancellationToken);

        return tags;
    }
}