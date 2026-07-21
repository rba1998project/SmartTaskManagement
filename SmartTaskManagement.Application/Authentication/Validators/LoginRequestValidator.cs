using FluentValidation;
using SmartTaskManagement.Application.Authentication.Dtos;

namespace SmartTaskManagement.Application.Authentication.Validators;

/// <summary>
/// Validates login input. Only presence is checked — credential correctness is
/// verified against Identity, and a wrong password must not be distinguishable here.
/// </summary>
public sealed class LoginRequestDtoValidator : AbstractValidator<LoginRequestDto>
{
    public LoginRequestDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email is required.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");
    }
}
