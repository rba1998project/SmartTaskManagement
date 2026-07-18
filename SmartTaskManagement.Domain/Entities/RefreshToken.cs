namespace SmartTaskManagement.Domain.Entities;

/// <summary>
/// A persisted refresh token. Only the SHA-256 hash of the raw token is stored — the raw
/// value is returned to the client once and never kept server-side. Tokens are rotated on
/// use and revocable (see the refresh-token service). Behavior lives here, not in a service,
/// so the entity is a genuine domain type rather than an anemic bag of properties.
/// </summary>
public class RefreshToken
{
    public Guid Id { get; private set; }

    //Owning Identity user id. No navigation to the Identity type keeps the Domain pure(following clean architecture principles).
    public Guid UserId { get; private set; }

    //SHA-256 hash of the raw token. The raw token is never persisted.
    public string TokenHash { get; private set; } = string.Empty;

    public DateTime ExpiresAt { get; private set; }

    public DateTime CreatedAt { get; private set; }

    //When the token was revoked (rotation or logout); null while still valid.
    public DateTime? RevokedAt { get; private set; }

    // EF Core materialization constructor.
    private RefreshToken() { }

    public RefreshToken(Guid userId, string tokenHash, DateTime createdAt, DateTime expiresAt)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User id is required.", nameof(userId));
        if (string.IsNullOrWhiteSpace(tokenHash))
            throw new ArgumentException("Token hash is required.", nameof(tokenHash));

        Id = Guid.NewGuid();
        UserId = userId;
        TokenHash = tokenHash;
        CreatedAt = createdAt;
        ExpiresAt = expiresAt;
    }

    public bool IsExpired(DateTime utcNow) => utcNow >= ExpiresAt;

    //Active means neither revoked nor expired — the only state accepted on refresh.
    public bool IsActive(DateTime utcNow) => RevokedAt is null && !IsExpired(utcNow);

    //Revokes the token (rotation or logout). Idempotent — the first revocation time is kept.
    public void Revoke(DateTime utcNow)
    {
        RevokedAt ??= utcNow;
    }
}
