using Microsoft.AspNetCore.Mvc;
using SmartTaskManagement.Application.Common;

namespace SmartTaskManagement.API.Common;

/// <summary>
/// Maps a failed <see cref="Result"/> onto an HTTP response, applying the
/// <see cref="ErrorType"/> contract: Validation → 400, Unauthorized → 401,
/// Forbidden → 403, NotFound → 404. An uncategorized failure falls back to
/// <paramref name="fallback"/> (bad request by default). This is the single place
/// the API translates Application outcomes into status codes, keeping controllers thin.
/// </summary>
public static class ResultToActionResult
{
    /// <summary>Builds the error <see cref="IActionResult"/> for a failed result.</summary>
    public static IActionResult ToErrorResponse(this Result result, string message, ErrorType fallback = ErrorType.Validation)
    {
        var payload = ApiResponse.Fail(message, result.Errors);

        return (result.ErrorType ?? fallback) switch
        {
            ErrorType.Unauthorized => new ObjectResult(payload) 
            { 
                StatusCode = StatusCodes.Status401Unauthorized 
            },

            ErrorType.Forbidden => new ObjectResult(payload) 
            { 
                StatusCode = StatusCodes.Status403Forbidden 
            },

            ErrorType.NotFound => new NotFoundObjectResult(payload),
            _ => new BadRequestObjectResult(payload),
        };
    }
}
