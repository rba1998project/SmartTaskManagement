namespace SmartTaskManagement.Application.Abstractions;

/// <summary>
/// Exposes the authenticated caller to the Application layer without leaking
/// <c>HttpContext</c> into it. Implemented by the API from the validated JWT claims.
/// Role membership is included because Application use cases enforce ownership rules
/// that differ by role (e.g. Admin may modify any project, a Project Manager only its own).
/// </summary>
public interface ICurrentUserService
{
    /// <summary>The authenticated user's id, or <c>null</c> when unauthenticated.</summary>
    Guid? UserId { get; }

    /// <summary>Whether the current request carries an authenticated user.</summary>
    bool IsAuthenticated { get; }

    /// <summary>Whether the authenticated user is a member of <paramref name="role"/>.</summary>
    bool IsInRole(string role);
}
