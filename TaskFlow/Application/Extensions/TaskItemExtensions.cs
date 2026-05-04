using TaskFlow.Application.DTOs;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Extensions;

public static class TaskItemExtensions
{
    public static TaskItemDto ToDto(this TaskItem task)
    {
        var subtasks = task.Subtasks
            .Select(s => new SubtaskDto(s.Id, s.Title, (int)s.Status, (int)s.Priority, s.CreatedAt))
            .ToList();

        return new TaskItemDto(
            task.Id,
            task.Title,
            task.Description,
            (int)task.Priority,
            (int)task.Status,
            task.Deadline,
            task.OrderIndex,
            task.CreatedAt,
            task.UpdatedAt,
            task.TaskTags.Select(tt => new TagDto(tt.Tag.Id, tt.Tag.Name, tt.Tag.Color)).ToList(),
            task.Comments.Select(c => new TaskCommentDto(c.Id, c.UserId, c.Content, c.CreatedAt)).ToList(),
            task.Attachments.Select(a => new TaskAttachmentDto(a.Id, a.FileName, a.FileSize, a.UploadedAt)).ToList(),
            task.ProjectId,
            task.ParentTaskId,
            subtasks);
    }
}
