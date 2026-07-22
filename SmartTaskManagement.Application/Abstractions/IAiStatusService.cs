using SmartTaskManagement.Application.Common;

namespace SmartTaskManagement.Application.Abstractions;

/// <summary>
/// Reports whether the AI description-improvement feature is currently available.
/// </summary>
public interface IAiStatusService
{
    /// <summary>
    /// Gets a value indicating whether the AI provider is configured and ready to use.
    /// </summary>
    bool IsAvailable { get; }
}
