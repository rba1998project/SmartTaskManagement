namespace SmartTaskManagement.Domain.Entities;

/// <summary>
/// A project owned by the user who created it. Ownership is represented solely by
/// <see cref="CreatedByUserId"/> (the Identity user id) — there is no navigation to the
/// Identity type, keeping the Domain free of Infrastructure/Identity dependencies.
/// Creation and update rules live here so the entity is a genuine domain type rather than
/// an anemic bag of properties. The current time is passed in by callers so the Domain
/// stays free of ambient <c>DateTime.UtcNow</c> and remains deterministic/testable.
/// </summary>
public class Project
{
    public Guid Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    // Owning Identity user id. No navigation to the Identity type keeps the Domain pure.
    public Guid CreatedByUserId { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    // EF Core materialization constructor.
    private Project() { }

    public Project(string name, string? description, Guid createdByUserId, DateTime createdAt)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Project name is required.", nameof(name));
        if (createdByUserId == Guid.Empty)
            throw new ArgumentException("Creator user id is required.", nameof(createdByUserId));

        Id = Guid.NewGuid();
        Name = name.Trim();
        Description = NormalizeDescription(description);
        CreatedByUserId = createdByUserId;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    /// <summary>
    /// Applies an update to the mutable fields and stamps <see cref="UpdatedAt"/>.
    /// Ownership never changes.
    /// </summary>
    public void Update(string name, string? description, DateTime updatedAt)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Project name is required.", nameof(name));

        Name = name.Trim();
        Description = NormalizeDescription(description);
        UpdatedAt = updatedAt;
    }

    // Empty/whitespace descriptions collapse to null so "absent" has one representation.
    private static string? NormalizeDescription(string? description) =>
        string.IsNullOrWhiteSpace(description) ? null : description.Trim();
}
