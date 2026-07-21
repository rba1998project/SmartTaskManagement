using FluentValidation;
using SmartTaskManagement.Application.Projects.Dtos;

namespace SmartTaskManagement.Application.Projects.Validators;

/// <summary>
/// Validates project creation input. Lengths mirror the persisted column sizes
/// (Name 200, Description 2000) so invalid input is rejected before it reaches the database.
/// </summary>
public sealed class CreateProjectRequestDtoValidator : AbstractValidator<CreateProjectRequestDto>
{
    public CreateProjectRequestDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Project name is required.")
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .MaximumLength(2000)
            .When(x => !string.IsNullOrEmpty(x.Description));
    }
}
