namespace SmartTaskManagement.Application.Projects.Dtos;

/// <summary>
/// Input for creating a project. The owner is taken from the authenticated caller, not the body.
/// </summary>
public sealed class CreateProjectRequest
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
}
