namespace TaskFlow.Domain.Entities;

public class Tag
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string UserId { get; set; }
    public string? Color { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public ICollection<TaskTag> TaskTags { get; set; } = new List<TaskTag>();
}