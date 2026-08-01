using FluentValidation;
using SmartTaskManagement.Application.Users.Dtos;

namespace SmartTaskManagement.Application.Users.Validators;

/// <summary>
/// Validates paged assignee lookup queries.
/// </summary>
public sealed class AssigneeQueryRequestDtoValidator : AbstractValidator<AssigneeQueryRequestDto>
{
    public AssigneeQueryRequestDtoValidator()
    {
        RuleFor(x => x.Search)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.Search));

        RuleFor(x => x.PageNumber)
            .GreaterThan(0);

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .LessThanOrEqualTo(100);
    }
}
