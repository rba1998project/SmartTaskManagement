using FluentValidation;
using SmartTaskManagement.Application.Tasks.Dtos;

namespace SmartTaskManagement.Application.Tasks.Validators;

/// <summary>
/// Validates AI description-improvement input. Length limit mirrors the persisted TaskItem
/// description column size (2000). The AI feature is optional; if the API key is missing,
/// the service returns a failure result rather than throwing.
/// </summary>
public sealed class ImproveTaskDescriptionRequestValidator : AbstractValidator<ImproveTaskDescriptionRequest>
{
    public ImproveTaskDescriptionRequestValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(2000).WithMessage("Description must be 2000 characters or fewer.");
    }
}
