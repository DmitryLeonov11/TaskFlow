using FluentValidation;

namespace TaskFlow.Features.Tasks.UpdateTask;

public class UpdateTaskValidator : AbstractValidator<UpdateTaskCommand>
{
    public UpdateTaskValidator()
    {
        RuleFor(x => x.TaskId)
            .NotEmpty().WithMessage("TaskId is required");

        RuleFor(x => x.Title.Value)
            .NotEmpty().WithMessage("Title cannot be empty")
            .MaximumLength(500).WithMessage("Title cannot exceed 500 characters")
            .When(x => x.Title.HasValue);

        RuleFor(x => x.Description.Value)
            .MaximumLength(5000).WithMessage("Description cannot exceed 5000 characters")
            .When(x => x.Description.HasValue && x.Description.Value != null);

        RuleFor(x => x.Priority.Value)
            .InclusiveBetween(0, 3).WithMessage("Priority must be between 0 and 3")
            .When(x => x.Priority.HasValue);

        RuleFor(x => x.Status.Value)
            .InclusiveBetween(0, 3).WithMessage("Status must be between 0 and 3")
            .When(x => x.Status.HasValue);

        RuleFor(x => x.Deadline.Value!.Value)
            .Must(d => d > DateTime.UtcNow).WithMessage("Deadline must be in the future")
            .When(x => x.Deadline.HasValue && x.Deadline.Value.HasValue);
    }
}
