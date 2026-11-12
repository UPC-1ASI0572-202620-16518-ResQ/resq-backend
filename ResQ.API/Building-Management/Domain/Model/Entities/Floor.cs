namespace ResQ.API.Building_Management.Domain.Model.Entities;

public class Floor
{
    public Guid Id { get; private set; }
    public Guid BuildingId { get; internal set; }
    public string Label { get; private set; } = string.Empty;
    public int Level { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    protected Floor() { }

    internal Floor(string label, int level)
    {
        Validate(label, level);
        var now = DateTimeOffset.UtcNow;
        Id = Guid.NewGuid();
        Label = label.Trim();
        Level = level;
        CreatedAt = now;
        UpdatedAt = now;
    }

    internal void Update(string label, int level)
    {
        Validate(label, level);
        Label = label.Trim();
        Level = level;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    private static void Validate(string label, int level)
    {
        if (string.IsNullOrWhiteSpace(label)) throw new ArgumentException("Floor label is required.");
        if (label.Trim().Length > 50) throw new ArgumentException("Floor label cannot exceed 50 characters.");
        if (level is < -20 or > 200) throw new ArgumentException("Floor level must be between -20 and 200.");
    }
}
