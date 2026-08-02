namespace SmartTaskManagement.Application.Users.Dtos;

/// <summary>
/// User representation for list views.
/// </summary>
public sealed record UserListItemDto(Guid Id, string Email, string? FullName, string Role);
