using Microsoft.AspNetCore.Identity;

namespace SmartTaskManagement.Infrastructure.Identity;

/// <summary>
/// Identity user with a Guid key. Lives in Infrastructure so the Domain stays free of
/// Identity dependencies. Extend with profile fields as later phases require.
/// </summary>
public class ApplicationUser : IdentityUser<Guid>
{
    /// <summary>Optional display name shown in the UI and embedded in the JWT <c>name</c> claim.</summary>
    public string? FullName { get; set; }
}
