using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.DTOs;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.Features.Attachments.GetAttachments;

public class GetAttachmentsHandler : IRequestHandler<GetAttachmentsQuery, IReadOnlyList<TaskAttachmentDto>>
{
    private readonly ApplicationDbContext _dbContext;

    public GetAttachmentsHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<TaskAttachmentDto>> Handle(GetAttachmentsQuery request, CancellationToken cancellationToken)
    {
        var attachments = await _dbContext.TaskAttachments
            .Where(a => a.TaskId == request.TaskId)
            .OrderByDescending(a => a.UploadedAt)
            .Select(a => new TaskAttachmentDto(
                a.Id,
                a.FileName,
                a.FileSize,
                a.UploadedAt
            ))
            .ToListAsync(cancellationToken);

        return attachments;
    }
}
