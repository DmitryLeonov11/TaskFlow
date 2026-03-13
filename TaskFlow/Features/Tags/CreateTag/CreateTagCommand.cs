using MediatR;
using TaskFlow.Application.DTOs;

namespace TaskFlow.Features.Tags.CreateTag;

public class CreateTagCommand : IRequest<TagDto>
{
    public required string Name { get; set; }
    public string? Color { get; set; }
    public required string UserId { get; set; }
}
