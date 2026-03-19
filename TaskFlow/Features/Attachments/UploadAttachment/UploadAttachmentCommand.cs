using MediatR;
using TaskFlow.Application.DTOs;

namespace TaskFlow.Features.Attachments.UploadAttachment;

public class UploadAttachmentCommand : IRequest<TaskAttachmentDto>
{
    public Guid TaskId { get; set; }
    public required string FileName { get; set; }
    public required string ContentType { get; set; }
    public long FileSize { get; set; }
    public required byte[] FileContent { get; set; }
    public required string UserId { get; set; }
    public required string UploadsFolder { get; set; }
}