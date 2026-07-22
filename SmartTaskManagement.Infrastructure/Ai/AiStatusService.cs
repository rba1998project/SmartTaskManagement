using Microsoft.Extensions.Options;
using SmartTaskManagement.Application.Abstractions;
using SmartTaskManagement.Infrastructure.Ai;

namespace SmartTaskManagement.Infrastructure.Ai;

/// <summary>
/// Determines AI availability from configuration.
/// </summary>
public sealed class AiStatusService : IAiStatusService
{
    private readonly GeminiOptions _options;

    /// <summary>
    /// Initializes a new instance of <see cref="AiStatusService"/>.
    /// </summary>
    /// <param name="options">AI configuration bound from the <c>Ai</c> section.</param>
    public AiStatusService(IOptions<GeminiOptions> options)
    {
        _options = options.Value;
    }

    /// <inheritdoc />
    public bool IsAvailable => !string.IsNullOrWhiteSpace(_options.ApiKey);
}
