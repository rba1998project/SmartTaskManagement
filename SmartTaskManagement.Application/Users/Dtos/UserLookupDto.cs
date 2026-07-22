namespace SmartTaskManagement.Application.Users.Dtos;

/// <summary>
/// Lightweight user representation for directory/selection UIs.
/// Distinct from <see cref="SmartTaskManagement.Application.Authentication.Models.AuthUser"/>:
/// this DTO is explicitly scoped to non-sensitive lookup data.
/// </summary>
public sealed record UserLookupDto(Guid Id, string FullName, string Email);
