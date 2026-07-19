using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SmartTaskManagement.API.Common;

namespace SmartTaskManagement.API.Filters;

/// <summary>
/// Runs any registered FluentValidation <see cref="IValidator{T}"/> against each action
/// argument before the action executes. On failure it short-circuits with HTTP 400 and the
/// standard <see cref="ApiResponse"/> error envelope — the same shape and status the
/// <c>ErrorType.Validation</c> mapping produces — so request validation is consistent across
/// the API. Arguments without a registered validator pass through untouched.
/// </summary>
public sealed class ValidationActionFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var services = context.HttpContext.RequestServices;
        var errors = new List<string>();

        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is null)
                continue;

            // Get the IValidator<T> type for the argument's type
            var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());

            // Attempt to resolve the validator from the DI container
            if (services.GetService(validatorType) is not IValidator validator)
                continue;

            // Create a ValidationContext for the argument and validate it
            var validationResult = await validator.ValidateAsync(new ValidationContext<object>(argument));

            // If validation fails, collect the error messages
            if (!validationResult.IsValid)
                errors.AddRange(validationResult.Errors.Select(e => e.ErrorMessage));
        }

        if (errors.Count > 0)
        {
            context.Result = new BadRequestObjectResult(
                ApiResponse.Fail("Validation failed.", errors));
            return;
        }

        await next();
    }
}
