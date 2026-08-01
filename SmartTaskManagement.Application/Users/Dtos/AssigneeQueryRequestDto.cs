namespace SmartTaskManagement.Application.Users.Dtos;

/// <summary>
/// Query parameters for the paged assignee lookup.
/// </summary>
public sealed class AssigneeQueryRequestDto
{
    /// <summary>Optional case-insensitive search across full name and email.</summary>
    public string? Search { get; init; }

    /// <summary>Page number (1-based).</summary>
    public int PageNumber { get; init; } = 1;

    /// <summary>Page size. Maximum 100.</summary>
    public int PageSize { get; init; } = 20;
}
