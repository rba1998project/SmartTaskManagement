namespace SmartTaskManagement.Application.Tasks.Dtos;

/// <summary>
/// Output from the AI description-improvement endpoint.
/// </summary>
public sealed class ImproveTaskDescriptionResponse
{
    /// <summary>
    /// The improved task description.
    /// </summary>
    public string ImprovedDescription { get; init; } = string.Empty;
}
