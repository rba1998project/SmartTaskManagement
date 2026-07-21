using FluentValidation;
using SmartTaskManagement.Application.Authentication.Dtos;

namespace SmartTaskManagement.Application.Authentication.Validators;

/// <summary>
/// Validates refresh input — the raw refresh token must be present.
/// </summary>
public sealed class RefreshRequestDtoValidator : AbstractValidator<RefreshRequestDto>
{
    public RefreshRequestDtoValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required.");
    }
}
