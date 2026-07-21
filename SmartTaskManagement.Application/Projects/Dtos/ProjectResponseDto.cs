namespace SmartTaskManagement.Application.Projects.Dtos;

/// <summary>
/// Project representation returned at the API boundary — keeps the Domain entity from leaking.
/// </summary>
public sealed class ProjectResponseDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public Guid CreatedByUserId { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
