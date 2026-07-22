namespace SmartTaskManagement.Infrastructure.Ai;

/// <summary>
/// Strongly-typed AI provider settings bound from the <c>Ai</c> configuration section.
/// </summary>
public sealed class GeminiOptions
{
    public const string SectionName = "Ai";

    /// <summary>
    /// The Gemini model identifier used for description improvement.
    /// </summary>
    public string Model { get; init; } = string.Empty;

    /// <summary>
    /// Base URL for the Gemini REST API.
    /// </summary>
    public string BaseUrl { get; init; } = string.Empty;

    /// <summary>
    /// Timeout for a single AI request.
    /// </summary>
    public TimeSpan Timeout { get; init; }

    /// <summary>
    /// The AI provider API key. Expected to come from User Secrets or environment variables,
    /// not from <c>appsettings.json</c>. Can be <c>null</c> or empty when AI is disabled.
    /// </summary>
    public string? ApiKey { get; init; }

    /// <summary>
    /// The HTTP header name used to send the API key to the provider.
    /// </summary>
    public string ApiKeyHeader { get; init; } = "X-goog-api-key";
}
