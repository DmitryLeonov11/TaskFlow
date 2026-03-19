using MediatR;
using TaskFlow.Application.DTOs;

namespace TaskFlow.Features.Comments.GetComments;

public class GetCommentsQuery : IRequest<IReadOnlyList<TaskCommentDto>>
{
    public Guid TaskId { get; set; }
}