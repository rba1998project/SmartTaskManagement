namespace SmartTaskManagement.Application.Users.Dtos;

/// <summary>
/// Request payload for updating a user's assigned role.
/// </summary>
public sealed record UpdateUserRoleRequest(string? RoleName);
