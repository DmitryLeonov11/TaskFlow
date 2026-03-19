using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.DTOs;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.Features.Attachments.UploadAttachment;

public class UploadAttachmentHandler : IRequestHandler<UploadAttachmentCommand, TaskAttachmentDto>
{
    private readonly ApplicationDbContext _dbContext;

    public UploadAttachmentHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TaskAttachmentDto> Handle(UploadAttachmentCommand request, CancellationToken cancellationToken)
    {
        var task = await _dbContext.Tasks
            .FirstOrDefaultAsync(t => t.Id == request.TaskId, cancellationToken);

        if (task == null)
        {
            throw new InvalidOperationException("Task not found");
        }

        if (task.UserId != request.UserId)
        {
            throw new UnauthorizedAccessException("You can only upload attachments to your own tasks");
        }

        // Create directory if not exists
        var taskFolder = Path.Combine(request.UploadsFolder, request.TaskId.ToString());
        if (!Directory.Exists(taskFolder))
        {
            Directory.CreateDirectory(taskFolder);
        }

        // Generate unique filename
        var uniqueFileName = $"{Guid.NewGuid()}_{request.FileName}";
        var filePath = Path.Combine(taskFolder, uniqueFileName);

        // Save file
        await File.WriteAllBytesAsync(filePath, request.FileContent, cancellationToken);

        var attachment = new TaskAttachment
        {
            Id = Guid.NewGuid(),
            TaskId = request.TaskId,
            FileName = request.FileName,
            FilePath = filePath,
            ContentType = request.ContentType,
            FileSize = request.FileSize,
            UploadedAt = DateTime.UtcNow
        };

        _dbContext.TaskAttachments.Add(attachment);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new TaskAttachmentDto(
            attachment.Id,
            attachment.FileName,
            attachment.FileSize,
            attachment.UploadedAt
        );
    }
}