using FluentValidation;
using SmartTaskManagement.Application.Tasks.Dtos;

namespace SmartTaskManagement.Application.Tasks.Validators;

/// <summary>
/// Validates <see cref="TaskQueryRequestDto"/> for server-side task list queries.
/// </summary>
public sealed class TaskQueryRequestDtoValidator : AbstractValidator<TaskQueryRequestDto>
{
    public TaskQueryRequestDtoValidator()
    {
        RuleFor(x => x.Search)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.Search));

        RuleFor(x => x.Status)
            .IsInEnum()
            .When(x => x.Status.HasValue);

        RuleFor(x => x.Priority)
            .IsInEnum()
            .When(x => x.Priority.HasValue);

        RuleFor(x => x.SortField)
            .IsInEnum();

        RuleFor(x => x.PageNumber)
            .GreaterThan(0);

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .LessThanOrEqualTo(100);
    }
}
