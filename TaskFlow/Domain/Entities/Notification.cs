namespace TaskFlow.Domain.Entities;

public enum NotificationType
{
    TaskAssigned = 0,
    CommentAdded = 1,
    TaskOverdue = 2,
    TaskMoved = 3,
    TaskCreated = 4
}

public class Notification
{
    public Guid Id { get; set; }
    public required string UserId { get; set; }
    public NotificationType Type { get; set; }
    public required string Message { get; set; }
    public Guid? RelatedTaskId { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}