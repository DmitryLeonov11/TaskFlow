namespace TaskFlow.Domain.Entities;

public enum TaskPriority
{
    Low = 0,
    Medium = 1,
    High = 2,
    Urgent = 3
}

public enum TaskStatus
{
    ToDo = 0,
    InProgress = 1,
    Review = 2,
    Done = 3
}

public class TaskItem
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public TaskPriority Priority { get; set; }
    public TaskStatus Status { get; set; }
    public DateTime? Deadline { get; set; }
    public int OrderIndex { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public required string UserId { get; set; }
    
    // Navigation properties
    public ICollection<TaskTag> TaskTags { get; set; } = new List<TaskTag>();
    public ICollection<TaskComment> Comments { get; set; } = new List<TaskComment>();
    public ICollection<TaskAttachment> Attachments { get; set; } = new List<TaskAttachment>();
}
