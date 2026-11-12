namespace ResQ.API.IAM.Domain.Model.Aggregates;

/// <summary>
/// Represents an organization that scopes application data and authorization.
/// </summary>
public class Organization
{
    public Guid Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public DateTime CreatedAt { get; private set; }

    private Organization()
    {
    }

    private Organization(Guid id, string name, DateTime createdAt)
    {
        Id = id;
        Name = name;
        CreatedAt = createdAt;
    }

    public static Organization Register(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Organization name is required.", nameof(name));

        name = name.Trim();

        if (name.Length > 120)
            throw new ArgumentException(
                "Organization name cannot exceed 120 characters.",
                nameof(name));

        return new Organization(
            Guid.NewGuid(),
            name,
            DateTime.UtcNow);
    }
}