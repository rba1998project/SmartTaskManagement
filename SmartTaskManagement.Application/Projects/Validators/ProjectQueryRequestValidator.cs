using FluentValidation;
using SmartTaskManagement.Application.Projects.Dtos;

namespace SmartTaskManagement.Application.Projects.Validators;

/// <summary>
/// Validates <see cref="ProjectQueryRequestDto"/> for server-side list queries.
/// </summary>
public sealed class ProjectQueryRequestDtoValidator : AbstractValidator<ProjectQueryRequestDto>
{
    public ProjectQueryRequestDtoValidator()
    {
        RuleFor(x => x.Search)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.Search));

        RuleFor(x => x.SortField)
            .IsInEnum();

        RuleFor(x => x.PageNumber)
            .GreaterThan(0);

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .LessThanOrEqualTo(100);
    }
}
