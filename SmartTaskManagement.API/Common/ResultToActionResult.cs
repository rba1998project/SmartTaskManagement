using Microsoft.AspNetCore.Mvc;
using SmartTaskManagement.Application.Common;

namespace SmartTaskManagement.API.Common;

/// <summary>
/// Maps a failed <see cref="Result"/> onto an HTTP response, applying the
/// <see cref="ErrorType"/> contract: Validation → 400, NotFound → 404, Forbidden → 403.
/// An uncategorized failure is treated as a bad request. This is the single place the
/// API translates Application outcomes into status codes, keeping controllers thin.
/// </summary>
public static class ResultToActionResult
{
    /// <summary>Builds the error <see cref="IActionResult"/> for a failed result.</summary>
    public static IActionResult ToErrorResponse(this Result result, string message)
    {
        var payload = ApiResponse.Fail(message, result.Errors);

        return result.ErrorType switch
        {
            ErrorType.NotFound => new NotFoundObjectResult(payload),
            ErrorType.Forbidden => new ObjectResult(payload) { StatusCode = StatusCodes.Status403Forbidden },
            _ => new BadRequestObjectResult(payload),
        };
    }
}
