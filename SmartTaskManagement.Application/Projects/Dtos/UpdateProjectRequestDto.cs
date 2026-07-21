namespace SmartTaskManagement.Application.Projects.Dtos;

/// <summary>
/// Input for updating a project's mutable fields. Ownership never changes through an update.
/// </summary>
public sealed class UpdateProjectRequestDto
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
}
