using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.Features.Attachments.DeleteAttachment;

public class DeleteAttachmentHandler : IRequestHandler<DeleteAttachmentCommand, Unit>
{
    private readonly ApplicationDbContext _dbContext;

    public DeleteAttachmentHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Unit> Handle(DeleteAttachmentCommand request, CancellationToken cancellationToken)
    {
        var attachment = await _dbContext.TaskAttachments
            .Include(a => a.Task)
            .FirstOrDefaultAsync(a => a.Id == request.AttachmentId, cancellationToken);

        if (attachment == null)
        {
            throw new InvalidOperationException("Attachment not found");
        }

        if (attachment.Task.UserId != request.UserId)
        {
            throw new UnauthorizedAccessException("You can only delete attachments from your own tasks");
        }

        // Delete physical file
        if (File.Exists(attachment.FilePath))
        {
            File.Delete(attachment.FilePath);
        }

        _dbContext.TaskAttachments.Remove(attachment);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}