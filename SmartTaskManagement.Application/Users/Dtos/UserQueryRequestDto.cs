using SmartTaskManagement.Application.Common;

namespace SmartTaskManagement.Application.Users.Dtos;

/// <summary>
/// Query parameters for the user list.
/// </summary>
public sealed class UserQueryRequestDto
{
    public string? Search { get; init; }

    public string? Role { get; init; }

    public UserSortField SortField { get; init; } = UserSortField.Email;

    public SortDirection SortDirection { get; init; } = SortDirection.Asc;

    public int PageNumber { get; init; } = 1;

    public int PageSize { get; init; } = 20;
}

public enum UserSortField
{
    Name = 0,
    Email = 1,
    Role = 2
}
