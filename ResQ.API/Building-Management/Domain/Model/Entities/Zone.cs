using ResQ.API.Building_Management.Domain.Model.ValueObjects;

namespace ResQ.API.Building_Management.Domain.Model.Entities;

/// <summary>
/// Physical area within a building (e.g. kitchen, warehouse, technical room).
/// Belongs to a single Building aggregate.
/// </summary>
public class Zone
{
    public Guid Id { get; private set; }
    public Guid BuildingId { get; internal set; }
    public ZoneCode ZoneCode { get; private set; } = null!;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? FloorLabel { get; private set; }
    public LocationAdministrativeStatus AdministrativeStatus { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    /// <summary>
    /// Required by EF Core.
    /// </summary>
    protected Zone() { }

    internal Zone(ZoneCode zoneCode, string name, string? description, string? floorLabel)
    {
        ValidateDetails(name, description, floorLabel);

        var now = DateTimeOffset.UtcNow;
        Id = Guid.NewGuid();
        ZoneCode = zoneCode;
        Name = name.Trim();
        Description = Normalize(description);
        FloorLabel = Normalize(floorLabel);
        AdministrativeStatus = LocationAdministrativeStatus.Active;
        CreatedAt = now;
        UpdatedAt = now;
    }

    internal void UpdateDetails(string name, string? description, string? floorLabel)
    {
        ValidateDetails(name, description, floorLabel);

        var normalizedName = name.Trim();
        var normalizedDescription = Normalize(description);
        var normalizedFloorLabel = Normalize(floorLabel);

        if (Name == normalizedName && Description == normalizedDescription && FloorLabel == normalizedFloorLabel)
            return;

        Name = normalizedName;
        Description = normalizedDescription;
        FloorLabel = normalizedFloorLabel;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    internal void ChangeAdministrativeStatus(LocationAdministrativeStatus newStatus)
    {
        if (AdministrativeStatus == newStatus)
            return;

        AdministrativeStatus = newStatus;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    private static void ValidateDetails(string name, string? description, string? floorLabel)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Zone name is required.");

        if (name.Trim().Length > 120)
            throw new ArgumentException("Zone name cannot exceed 120 characters.");

        if (description?.Trim().Length > 500)
            throw new ArgumentException("Zone description cannot exceed 500 characters.");

        if (floorLabel?.Trim().Length > 50)
            throw new ArgumentException("Floor label cannot exceed 50 characters.");
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
