using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.Features.Tags.DeleteTag;

public class DeleteTagHandler : IRequestHandler<DeleteTagCommand, bool>
{
    private readonly ApplicationDbContext _dbContext;

    public DeleteTagHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> Handle(DeleteTagCommand request, CancellationToken cancellationToken)
    {
        var tag = await _dbContext.Tags
            .FirstOrDefaultAsync(t => t.Id == request.TagId && t.UserId == request.UserId, cancellationToken);

        if (tag is null)
            return false;

        _dbContext.Tags.Remove(tag);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}
