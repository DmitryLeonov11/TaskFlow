namespace TaskFlow.Domain.Entities;

public class TaskAttachment
{
    public Guid Id { get; set; }
    public Guid TaskId { get; set; }
    public required string FileName { get; set; }
    public required string FilePath { get; set; }
    public required string ContentType { get; set; }
    public long FileSize { get; set; }
    public DateTime UploadedAt { get; set; }
    
    // Navigation properties
    public TaskItem Task { get; set; } = null!;
}
