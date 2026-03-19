using FluentValidation;
using TaskFlow.Features.Tasks.UpdateTask;

namespace TaskFlow.Features.Tasks.UpdateTask;

public class UpdateTaskValidator : AbstractValidator<UpdateTaskCommand>
{
    public UpdateTaskValidator()
    {
        RuleFor(x => x.TaskId)
            .NotEmpty().WithMessage("TaskId is required");

        RuleFor(x => x.Title)
            .MaximumLength(500).WithMessage("Title cannot exceed 500 characters")
            .When(x => x.Title != null);

        RuleFor(x => x.Description)
            .MaximumLength(5000).WithMessage("Description cannot exceed 5000 characters")
            .When(x => x.Description != null);

        RuleFor(x => x.Priority)
            .InclusiveBetween(0, 3).WithMessage("Priority must be between 0 and 3")
            .When(x => x.Priority.HasValue);

        RuleFor(x => x.Status)
            .InclusiveBetween(0, 3).WithMessage("Status must be between 0 and 3")
            .When(x => x.Status.HasValue);

        RuleFor(x => x.Deadline)
            .GreaterThan(DateTime.UtcNow).WithMessage("Deadline must be in the future")
            .When(x => x.Deadline.HasValue);
    }
}