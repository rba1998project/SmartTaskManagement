using FluentValidation;
using SmartTaskManagement.Application.Authorization;
using SmartTaskManagement.Application.Users.Dtos;

namespace SmartTaskManagement.Application.Users.Validators;

public sealed class UserQueryRequestValidator : AbstractValidator<UserQueryRequestDto>
{
    public UserQueryRequestValidator()
    {
        RuleFor(x => x.Search)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.Search));

        RuleFor(x => x.Role)
            .Must(role => string.IsNullOrWhiteSpace(role) || RoleNames.All.Contains(role))
            .WithMessage("Role is invalid.");

        RuleFor(x => x.PageNumber).GreaterThan(0);
        RuleFor(x => x.PageSize).GreaterThan(0).LessThanOrEqualTo(100);
        RuleFor(x => x.SortField).IsInEnum();
        RuleFor(x => x.SortDirection).IsInEnum();
    }
}
