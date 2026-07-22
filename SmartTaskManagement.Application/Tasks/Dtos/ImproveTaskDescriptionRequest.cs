namespace SmartTaskManagement.Application.Tasks.Dtos;

/// <summary>
/// Input for improving a task description using AI.
/// </summary>
public sealed class ImproveTaskDescriptionRequest
{
    /// <summary>
    /// The raw task description to improve.
    /// </summary>
    public string Description { get; init; } = string.Empty;
}
