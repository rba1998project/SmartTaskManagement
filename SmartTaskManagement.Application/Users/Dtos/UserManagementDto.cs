namespace SmartTaskManagement.Application.Users.Dtos;

/// <summary>
/// User representation for management UIs.
/// </summary>
public sealed record UserManagementDto(Guid Id, string Email, string? FullName, string Role);
