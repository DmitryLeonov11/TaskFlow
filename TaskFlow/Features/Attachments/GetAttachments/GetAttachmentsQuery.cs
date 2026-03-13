using MediatR;
using TaskFlow.Application.DTOs;

namespace TaskFlow.Features.Attachments.GetAttachments;

public class GetAttachmentsQuery : IRequest<IReadOnlyList<TaskAttachmentDto>>
{
    public Guid TaskId { get; set; }
}
