namespace TaskFlow.Domain.Entities;

public class TaskTag
{
    public Guid TaskId { get; set; }
    public Guid TagId { get; set; }
    
    // Navigation properties
    public TaskItem Task { get; set; } = null!;
    public Tag Tag { get; set; } = null!;
}
