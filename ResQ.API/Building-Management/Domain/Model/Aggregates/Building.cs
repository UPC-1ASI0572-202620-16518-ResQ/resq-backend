using ResQ.API.Building_Management.Domain.Model.Entities;
using ResQ.API.Building_Management.Domain.Model.ValueObjects;

namespace ResQ.API.Building_Management.Domain.Model.Aggregates;

/// <summary>
/// Aggregate Root representing a building owned by an organization.
/// All zone mutations are performed through this aggregate.
/// </summary>
public class Building
{
    private readonly List<Zone> _zones = new();

    public Guid Id { get; private set; }
    public Guid OrganizationId { get; private set; }
    public BuildingCode BuildingCode { get; private set; } = null!;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public BuildingAddress Address { get; private set; } = null!;
    public LocationAdministrativeStatus AdministrativeStatus { get; private set; }
    public IReadOnlyCollection<Zone> Zones => _zones.AsReadOnly();
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public long Version { get; private set; }

    /// <summary>
    /// Required by EF Core.
    /// </summary>
    protected Building() { }

    /// <summary>
    /// Registers a new active building with no zones.
    /// </summary>
    public static Building Register(Guid organizationId, BuildingCode buildingCode,
        string name, string? description, BuildingAddress address)
    {
        if (organizationId == Guid.Empty)
            throw new ArgumentException("Organization id is required.");

        ArgumentNullException.ThrowIfNull(buildingCode);
        ArgumentNullException.ThrowIfNull(address);
        ValidateDetails(name, description);

        var now = DateTimeOffset.UtcNow;

        return new Building
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            BuildingCode = buildingCode,
            Name = name.Trim(),
            Description = Normalize(description),
            Address = address,
            AdministrativeStatus = LocationAdministrativeStatus.Active,
            CreatedAt = now,
            UpdatedAt = now,
            Version = 1
        };
    }

    /// <summary>
    /// Updates the descriptive information and address.
    /// </summary>
    public void UpdateDetails(string name, string? description, BuildingAddress address)
    {
        ArgumentNullException.ThrowIfNull(address);
        ValidateDetails(name, description);

        var normalizedName = name.Trim();
        var normalizedDescription = Normalize(description);

        if (Name == normalizedName && Description == normalizedDescription && Address == address)
            return;

        Name = normalizedName;
        Description = normalizedDescription;
        Address = address;

        RegisterChange();
    }

    /// <summary>
    /// Activates or deactivates the building administratively.
    /// </summary>
    public void ChangeAdministrativeStatus(LocationAdministrativeStatus status)
    {
        if (AdministrativeStatus == status)
            return;

        AdministrativeStatus = status;
        RegisterChange();
    }

    /// <summary>
    /// Adds an active zone to this building. The building must be active.
    /// </summary>
    public Zone AddZone(string zoneCodeValue, string name, string? description, string? floorLabel)
    {
        if (AdministrativeStatus != LocationAdministrativeStatus.Active)
            throw new InvalidOperationException("Cannot add a zone to an inactive building.");

        var zoneCode = ZoneCode.Create(zoneCodeValue);

        if (_zones.Any(z => z.ZoneCode.Value == zoneCode.Value))
            throw new InvalidOperationException($"A zone with code '{zoneCode.Value}' already exists in this building.");

        var zone = new Zone(zoneCode, name, description, floorLabel)
        {
            BuildingId = Id
        };

        _zones.Add(zone);
        RegisterChange();

        return zone;
    }

    /// <summary>
    /// Updates the descriptive data of a zone belonging to this building.
    /// </summary>
    public void UpdateZone(Guid zoneId, string name, string? description, string? floorLabel)
    {
        var zone = FindZoneOrThrow(zoneId);
        zone.UpdateDetails(name, description, floorLabel);
        RegisterChange();
    }

    /// <summary>
    /// Changes the administrative status of a zone. Activation requires an active building.
    /// </summary>
    public void ChangeZoneAdministrativeStatus(Guid zoneId, LocationAdministrativeStatus status)
    {
        if (status == LocationAdministrativeStatus.Active &&
            AdministrativeStatus != LocationAdministrativeStatus.Active)
        {
            throw new InvalidOperationException("Cannot activate a zone in an inactive building.");
        }

        var zone = FindZoneOrThrow(zoneId);
        zone.ChangeAdministrativeStatus(status);
        RegisterChange();
    }

    /// <summary>
    /// Checks whether the building (and optionally a zone) is available for device assignment.
    /// </summary>
    public bool IsAvailableForAssignment(Guid? zoneId = null)
    {
        if (AdministrativeStatus != LocationAdministrativeStatus.Active)
            return false;

        if (zoneId == null)
            return true;

        var zone = _zones.FirstOrDefault(z => z.Id == zoneId.Value);
        return zone?.AdministrativeStatus == LocationAdministrativeStatus.Active;
    }

    private Zone FindZoneOrThrow(Guid zoneId)
    {
        var zone = _zones.FirstOrDefault(z => z.Id == zoneId);
        if (zone == null)
            throw new KeyNotFoundException($"Zone '{zoneId}' was not found in this building.");
        return zone;
    }

    private static void ValidateDetails(string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Building name is required.");

        if (name.Trim().Length > 120)
            throw new ArgumentException("Building name cannot exceed 120 characters.");

        if (description?.Trim().Length > 500)
            throw new ArgumentException("Building description cannot exceed 500 characters.");
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private void RegisterChange()
    {
        UpdatedAt = DateTimeOffset.UtcNow;
        Version++;
    }
}
