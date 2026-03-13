using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.Features.Attachments.DownloadAttachment;

public class DownloadAttachmentHandler : IRequestHandler<DownloadAttachmentQuery, (byte[] Content, string ContentType, string FileName)>
{
    private readonly ApplicationDbContext _dbContext;

    public DownloadAttachmentHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<(byte[] Content, string ContentType, string FileName)> Handle(DownloadAttachmentQuery request, CancellationToken cancellationToken)
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
            throw new UnauthorizedAccessException("You can only download attachments from your own tasks");
        }

        if (!File.Exists(attachment.FilePath))
        {
            throw new FileNotFoundException("Attachment file not found on server");
        }

        var content = await File.ReadAllBytesAsync(attachment.FilePath, cancellationToken);

        return (content, attachment.ContentType, attachment.FileName);
    }
}
