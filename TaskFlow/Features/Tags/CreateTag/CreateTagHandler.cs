using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.DTOs;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.Features.Tags.CreateTag;

public class CreateTagHandler : IRequestHandler<CreateTagCommand, TagDto>
{
    private readonly ApplicationDbContext _dbContext;

    public CreateTagHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TagDto> Handle(CreateTagCommand request, CancellationToken cancellationToken)
    {
        var existingTag = await _dbContext.Tags
            .FirstOrDefaultAsync(t => t.Name == request.Name && t.UserId == request.UserId, cancellationToken);

        if (existingTag != null)
            throw new InvalidOperationException($"Tag '{request.Name}' already exists");

        var tag = new Tag
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            UserId = request.UserId,
            Color = request.Color,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Tags.Add(tag);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new TagDto(tag.Id, tag.Name, tag.Color);
    }
}