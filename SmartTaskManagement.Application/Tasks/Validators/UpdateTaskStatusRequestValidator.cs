using FluentValidation;
using SmartTaskManagement.Application.Tasks.Dtos;

namespace SmartTaskManagement.Application.Tasks.Validators;

/// <summary>
/// Validates task status-change input. Status must be a defined enum value so an out-of-range
/// value is rejected before it reaches the service.
/// </summary>
public sealed class UpdateTaskStatusRequestDtoValidator : AbstractValidator<UpdateTaskStatusRequestDto>
{
    public UpdateTaskStatusRequestDtoValidator()
    {
        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Status is not a valid value.");

        RuleFor(x => x.Comment)
            .Must(comment => !string.IsNullOrWhiteSpace(comment))
            .WithMessage("Comment is required.")
            .MaximumLength(1000)
            .WithMessage("Comment must be at most 1000 characters.");
    }
}
