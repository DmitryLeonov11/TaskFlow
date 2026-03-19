namespace TaskFlow.Application.DTOs;

public record TaskItemDto(
    Guid Id,
    string Title,
    string? Description,
    int Priority,
    int Status,
    DateTime? Deadline,
    int OrderIndex,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IReadOnlyList<TagDto> Tags,
    IReadOnlyList<TaskCommentDto> Comments,
    IReadOnlyList<TaskAttachmentDto> Attachments);

public record PagedResponse<T>(
    T Data,
    int TotalCount,
    int PageNumber,
    int PageSize);

public record CreateTaskDto(
    string Title,
    string? Description,
    int Priority,
    DateTime? Deadline,
    IReadOnlyList<Guid>? TagIds);

public record UpdateTaskDto(
    string? Title,
    string? Description,
    int? Priority,
    int? Status,
    DateTime? Deadline,
    IReadOnlyList<Guid>? TagIds);

public record MoveTaskDto(
    int NewStatus,
    int NewOrderIndex);

public record TagDto(
    Guid Id,
    string Name,
    string? Color);

public record CreateTagDto(
    string Name,
    string? Color);

public record TaskCommentDto(
    Guid Id,
    string UserId,
    string Content,
    DateTime CreatedAt);

public record CreateCommentDto(
    string Content);

public record TaskAttachmentDto(
    Guid Id,
    string FileName,
    long FileSize,
    DateTime UploadedAt);