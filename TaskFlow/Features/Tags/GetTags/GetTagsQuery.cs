using MediatR;
using TaskFlow.Application.DTOs;

namespace TaskFlow.Features.Tags.GetTags;

public class GetTagsQuery : IRequest<IReadOnlyList<TagDto>>
{
    public required string UserId { get; set; }
}
