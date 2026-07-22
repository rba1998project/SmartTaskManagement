using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using SmartTaskManagement.Application.Abstractions;
using SmartTaskManagement.Application.Common;
using SmartTaskManagement.Infrastructure.Ai;

namespace SmartTaskManagement.Infrastructure.Ai;

/// <summary>
/// Improves task descriptions by calling the Gemini REST API using header-based API key authentication.
/// </summary>
public sealed class GeminiTaskAiService : ITaskAiService
{
    private readonly HttpClient _httpClient;
    private readonly GeminiOptions _options;
    private readonly string? _apiKey;

    /// <summary>
    /// Initializes a new instance of <see cref="GeminiTaskAiService"/>.
    /// </summary>
    /// <param name="httpClient">Typed client provided by IHttpClientFactory.</param>
    /// <param name="options">Gemini configuration bound from the <c>Ai</c> section.</param>
    public GeminiTaskAiService(HttpClient httpClient, IOptions<GeminiOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _apiKey = options.Value.ApiKey;
        _httpClient.Timeout = _options.Timeout;
    }

    /// <inheritdoc />
    public async Task<Result<string>> ImproveDescriptionAsync(string description, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return Result<string>.Failure("Description must not be empty.");
        }

        if (description.Length > 2000)
        {
            return Result<string>.Failure("Description must be 2000 characters or fewer.");
        }

        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            return Result<string>.Failure("AI description improvement is not configured.");
        }

        try
        {
            var prompt = $"{AiPrompts.SystemInstruction}\n\n{AiPrompts.BuildUserPrompt(description)}";

            var requestUri = $"{_options.BaseUrl}/models/{_options.Model}:generateContent";

            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                }
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, requestUri)
            {
                Content = JsonContent.Create(requestBody)
            };

            request.Headers.Add(_options.ApiKeyHeader, _apiKey);

            using var response = await _httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);

                return Result<string>.Failure("The AI provider returned an error. Please try again later.");
            }

            var geminiResponse = await response.Content.ReadFromJsonAsync<GeminiResponse>(cancellationToken: cancellationToken);

            if (geminiResponse?.Candidates is not { Length: > 0 })
            {
                return Result<string>.Failure("The AI provider returned an empty response. Please try again.");
            }

            var improvedText = geminiResponse.Candidates[0].Content?.Parts?.FirstOrDefault()?.Text;

            if (string.IsNullOrWhiteSpace(improvedText))
            {
                return Result<string>.Failure("The AI provider returned an empty description. Please try again.");
            }

            return Result<string>.Success(improvedText.Trim());
        }
        catch (OperationCanceledException)
        {
            return Result<string>.Failure("The AI request timed out. Please try again.");
        }
        catch (HttpRequestException)
        {
            return Result<string>.Failure("Unable to reach the AI provider. Please try again later.");
        }
        catch (Exception)
        {
            return Result<string>.Failure("An unexpected error occurred while improving the description. Please try again.");
        }
    }

    private sealed class GeminiResponse
    {
        public GeminiCandidate[]? Candidates { get; init; }
    }

    private sealed class GeminiCandidate
    {
        public GeminiContent? Content { get; init; }
    }

    private sealed class GeminiContent
    {
        public GeminiPart[]? Parts { get; init; }
    }

    private sealed class GeminiPart
    {
        public string? Text { get; init; }
    }
}
