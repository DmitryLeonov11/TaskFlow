using TaskFlow.Application.Common;

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
    IReadOnlyList<TaskAttachmentDto> Attachments,
    Guid? ProjectId = null,
    Guid? ParentTaskId = null,
    IReadOnlyList<SubtaskDto>? Subtasks = null);

public record SubtaskDto(
    Guid Id,
    string Title,
    int Status,
    int Priority,
    DateTime CreatedAt);

public record ProjectDto(
    Guid Id,
    string Name,
    string? Description,
    string? Color,
    DateTime CreatedAt,
    int TaskCount = 0);

public record CreateProjectDto(
    string Name,
    string? Description,
    string? Color);

public record UpdateProjectDto(
    string? Name,
    string? Description,
    string? Color);

public record AdminUserDto(
    string Id,
    string Email,
    string? FirstName,
    string? LastName,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? LastLoginAt,
    IReadOnlyList<string> Roles);

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
    IReadOnlyList<Guid>? TagIds,
    Guid? ProjectId = null,
    Guid? ParentTaskId = null);

public record UpdateTaskDto(
    Optional<string> Title,
    Optional<string?> Description,
    Optional<int> Priority,
    Optional<int> Status,
    Optional<DateTime?> Deadline,
    Optional<IReadOnlyList<Guid>?> TagIds);

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