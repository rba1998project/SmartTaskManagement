using SmartTaskManagement.Application.Common;

namespace SmartTaskManagement.Application.Abstractions;

/// <summary>
/// Improves task descriptions using an AI provider.
/// </summary>
public interface ITaskAiService
{
    /// <summary>
    /// Returns an improved version of the supplied task description while
    /// preserving its original intent.
    /// </summary>
    /// <param name="description">
    /// The task description to improve.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the operation.
    /// </param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing the improved description on success,
    /// or one or more errors if the operation fails.
    /// </returns>
    Task<Result<string>> ImproveDescriptionAsync(
        string description,
        CancellationToken cancellationToken = default);
}
