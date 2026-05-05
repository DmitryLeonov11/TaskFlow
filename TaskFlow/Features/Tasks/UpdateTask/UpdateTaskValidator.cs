using FluentValidation;

namespace TaskFlow.Features.Tasks.UpdateTask;

public class UpdateTaskValidator : AbstractValidator<UpdateTaskCommand>
{
    public UpdateTaskValidator()
    {
        RuleFor(x => x.TaskId)
            .NotEmpty().WithMessage("TaskId is required");

        RuleFor(x => x.Title)
            .Must(t => !t.HasValue || !string.IsNullOrWhiteSpace(t.Value))
            .WithMessage("Title cannot be empty")
            .Must(t => !t.HasValue || (t.Value?.Length ?? 0) <= 500)
            .WithMessage("Title cannot exceed 500 characters");

        RuleFor(x => x.Description)
            .Must(d => !d.HasValue || d.Value == null || d.Value.Length <= 5000)
            .WithMessage("Description cannot exceed 5000 characters");

        RuleFor(x => x.Priority)
            .Must(p => !p.HasValue || (p.Value >= 0 && p.Value <= 3))
            .WithMessage("Priority must be between 0 and 3");

        RuleFor(x => x.Status)
            .Must(s => !s.HasValue || (s.Value >= 0 && s.Value <= 3))
            .WithMessage("Status must be between 0 and 3");

        RuleFor(x => x.Deadline)
            .Must(d => !d.HasValue || !d.Value.HasValue || d.Value.Value > DateTime.UtcNow)
            .WithMessage("Deadline must be in the future");
    }
}
