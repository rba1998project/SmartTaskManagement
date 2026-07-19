namespace SmartTaskManagement.Application.Authentication.Models;

/// <summary>
/// Application-neutral view of an authenticated user. Lets the Application layer
/// orchestrate auth without referencing the Infrastructure Identity types.
/// </summary>
public sealed record AuthUser(Guid Id, string Email, string? FullName);
