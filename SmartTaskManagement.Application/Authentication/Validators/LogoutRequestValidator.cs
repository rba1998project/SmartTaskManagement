using FluentValidation;
using SmartTaskManagement.Application.Authentication.Dtos;

namespace SmartTaskManagement.Application.Authentication.Validators;

/// <summary>
/// Validates logout input — the raw refresh token to revoke must be present.
/// </summary>
public sealed class LogoutRequestValidator : AbstractValidator<LogoutRequest>
{
    public LogoutRequestValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required.");
    }
}
