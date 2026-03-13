using FluentValidation;

namespace TaskFlow.Features.Attachments.UploadAttachment;

public class UploadAttachmentValidator : AbstractValidator<UploadAttachmentCommand>
{
    private const int MaxFileSize = 10 * 1024 * 1024; // 10 MB

    public UploadAttachmentValidator()
    {
        RuleFor(x => x.TaskId)
            .NotEmpty().WithMessage("TaskId is required");

        RuleFor(x => x.FileName)
            .NotEmpty().WithMessage("FileName is required")
            .MaximumLength(255).WithMessage("FileName cannot exceed 255 characters")
            .Must(HaveValidExtension).WithMessage("Invalid file extension");

        RuleFor(x => x.ContentType)
            .NotEmpty().WithMessage("ContentType is required");

        RuleFor(x => x.FileSize)
            .GreaterThan(0).WithMessage("FileSize must be greater than 0")
            .LessThanOrEqualTo(MaxFileSize).WithMessage($"File size cannot exceed {MaxFileSize / 1024 / 1024} MB");

        RuleFor(x => x.FileContent)
            .NotEmpty().WithMessage("FileContent is required");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required");
    }

    private bool HaveValidExtension(string fileName)
    {
        var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".png", ".jpg", ".jpeg", ".gif", ".txt", ".zip", ".rar" };
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return allowedExtensions.Contains(extension);
    }
}
