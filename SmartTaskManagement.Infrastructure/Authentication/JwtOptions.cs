namespace SmartTaskManagement.Infrastructure.Authentication;

/// <summary>
/// Strongly-typed JWT settings bound from the <c>Jwt</c> configuration section.
/// Holds non-secret values only; the signing key lives in User Secrets / environment variables.
/// JWT issuance is an Infrastructure concern, so these settings live alongside the token generator.
/// </summary>
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    //Token issuer (the API).
    public string Issuer { get; init; } = string.Empty;

    //Intended token audience (the client).
    public string Audience { get; init; } = string.Empty;

    //Access token lifetime in minutes.
    public int AccessTokenMinutes { get; init; }

    //Refresh token lifetime in days.
    public int RefreshTokenDays { get; init; }
}
