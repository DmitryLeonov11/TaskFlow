using MediatR;

namespace TaskFlow.Features.Attachments.DownloadAttachment;

public class DownloadAttachmentQuery : IRequest<(byte[] Content, string ContentType, string FileName)>
{
    public Guid AttachmentId { get; set; }
    public required string UserId { get; set; }
}