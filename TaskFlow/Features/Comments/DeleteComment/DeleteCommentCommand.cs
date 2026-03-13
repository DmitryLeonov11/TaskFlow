using MediatR;

namespace TaskFlow.Features.Comments.DeleteComment;

public class DeleteCommentCommand : IRequest<Unit>
{
    public Guid CommentId { get; set; }
    public required string UserId { get; set; }
}
