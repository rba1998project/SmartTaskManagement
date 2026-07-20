using FluentValidation;
using SmartTaskManagement.Application.Tasks.Dtos;

namespace SmartTaskManagement.Application.Tasks.Validators;

/// <summary>
/// Validates task update input. Lengths mirror the persisted column sizes (Title 200,
/// Description 2000). Priority must be a defined enum value, and a due date, when supplied,
/// must be in the future.
/// </summary>
public sealed class UpdateTaskRequestValidator : AbstractValidator<UpdateTaskRequest>
{
    public UpdateTaskRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Task title is required.")
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .MaximumLength(2000)
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.Priority)
            .IsInEnum().WithMessage("Priority is not a valid value.");

        RuleFor(x => x.DueDate)
            .GreaterThan(DateTime.UtcNow).WithMessage("Due date must be in the future.")
            .When(x => x.DueDate.HasValue);
    }
}
