using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.Features.Comments.DeleteComment;

public class DeleteCommentHandler : IRequestHandler<DeleteCommentCommand, Unit>
{
    private readonly ApplicationDbContext _dbContext;

    public DeleteCommentHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Unit> Handle(DeleteCommentCommand request, CancellationToken cancellationToken)
    {
        var comment = await _dbContext.TaskComments
            .FirstOrDefaultAsync(c => c.Id == request.CommentId, cancellationToken);

        if (comment == null)
        {
            throw new InvalidOperationException("Comment not found");
        }

        if (comment.UserId != request.UserId)
        {
            throw new UnauthorizedAccessException("You can only delete your own comments");
        }

        _dbContext.TaskComments.Remove(comment);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
