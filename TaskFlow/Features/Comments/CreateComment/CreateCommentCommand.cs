using MediatR;
using TaskFlow.Application.DTOs;

namespace TaskFlow.Features.Comments.CreateComment;

public class CreateCommentCommand : IRequest<TaskCommentDto>
{
    public Guid TaskId { get; set; }
    public required string Content { get; set; }
    public required string UserId { get; set; }
}