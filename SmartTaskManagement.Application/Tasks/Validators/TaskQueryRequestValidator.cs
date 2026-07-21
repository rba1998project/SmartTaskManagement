using FluentValidation;
using SmartTaskManagement.Application.Tasks.Dtos;

namespace SmartTaskManagement.Application.Tasks.Validators;

/// <summary>
/// Validates <see cref="TaskQueryRequestDto"/> for server-side task list queries.
/// Placeholder for Phase 5C.
/// </summary>
public sealed class TaskQueryRequestDtoValidator : AbstractValidator<TaskQueryRequestDto>
{
    public TaskQueryRequestDtoValidator()
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
