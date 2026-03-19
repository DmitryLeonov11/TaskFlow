using MediatR;

namespace TaskFlow.Features.Attachments.DeleteAttachment;

public class DeleteAttachmentCommand : IRequest<Unit>
{
    public Guid AttachmentId { get; set; }
    public required string UserId { get; set; }
}