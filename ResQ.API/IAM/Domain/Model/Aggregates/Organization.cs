namespace ResQ.API.IAM.Domain.Model.Aggregates;

/// <summary>
///     Organization (tenant) that owns users and all the data of the other bounded contexts.
/// </summary>
public class Organization
{
    public Guid Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public DateTime CreatedAt { get; private set; }

    protected Organization()
    {
    }

    /// <summary>
    ///     Registers a new organization with a generated id.
    /// </summary>
    /// <param name="name">Display name of the organization (1-120 characters).</param>
    public static Organization Register(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Organization name is required.", nameof(name));

        var trimmed = name.Trim();
        if (trimmed.Length > 120)
            throw new ArgumentException("Organization name cannot exceed 120 characters.", nameof(name));

        return new Organization
        {
            Id = Guid.NewGuid(),
            Name = trimmed,
            CreatedAt = DateTime.UtcNow
        };
    }
}
