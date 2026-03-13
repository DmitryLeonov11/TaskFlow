namespace TaskFlow.Application.DTOs;

public record NotificationDto(
    Guid Id,
    string Type,
    string Message,
    Guid? RelatedTaskId,
    bool IsRead,
    DateTime CreatedAt);

public record GetNotificationsResponse(
    IReadOnlyList<NotificationDto> Notifications,
    int TotalCount,
    int UnreadCount);
