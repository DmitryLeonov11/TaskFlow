using FluentValidation;
using TaskFlow.Features.Tasks.MoveTask;

namespace TaskFlow.Features.Tasks.MoveTask;

public class MoveTaskValidator : AbstractValidator<MoveTaskCommand>
{
    public MoveTaskValidator()
    {
        RuleFor(x => x.TaskId)
            .NotEmpty().WithMessage("TaskId is required");

        RuleFor(x => x.NewStatus)
            .InclusiveBetween(0, 3).WithMessage("Status must be between 0 and 3");

        RuleFor(x => x.NewOrderIndex)
            .GreaterThanOrEqualTo(0).WithMessage("OrderIndex cannot be negative");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required");
    }
}