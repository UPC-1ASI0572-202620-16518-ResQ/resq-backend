using ResQ.API.Device_Management.Domain.Model.Entities;
using ResQ.API.Device_Management.Domain.Model.ValueObjects;

namespace ResQ.API.Device_Management.Domain.Model.Aggregates;

/// <summary>
/// Aggregate Root that represents an IoT Device registered in ResQ.
/// </summary>
public class Device
{
    private readonly List<DeviceCapability> _capabilities = new();

    /// <summary>
    /// Unique identifier of the device.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Organization that owns the device.
    /// </summary>
    public Guid OrganizationId { get; private set; }

    /// <summary>
    /// Unique device code inside the organization.
    /// </summary>
    public DeviceCode DeviceCode { get; private set; } = null!;

    /// <summary>
    /// Display name of the device.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Optional administrative description.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Manufacturer, model and serial number information.
    /// </summary>
    public DeviceSpecifications Specifications { get; private set; } = null!;

    /// <summary>
    /// Current building and optional zone assignment.
    /// </summary>
    public DeviceAssignment Assignment { get; private set; } = null!;

    /// <summary>
    /// Optional reference to the device in an external system.
    /// </summary>
    public ExternalDeviceReference? ExternalReference { get; private set; }

    /// <summary>
    /// Administrative status of the device.
    /// </summary>
    public EDeviceAdministrativeStatus AdministrativeStatus { get; private set; }

    /// <summary>
    /// Measurement and actuation capabilities of the device.
    /// </summary>
    public IReadOnlyCollection<DeviceCapability> Capabilities =>
        _capabilities.AsReadOnly();

    /// <summary>
    /// Date and time when the device was registered.
    /// </summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>
    /// Date and time of the last modification.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; private set; }

    /// <summary>
    /// Version used for optimistic concurrency control.
    /// </summary>
    public long Version { get; private set; }

    /// <summary>
    /// Required by Entity Framework Core.
    /// </summary>
    protected Device()
    {
    }

    /// <summary>
    /// Registers a new inactive device.
    /// </summary>
    public static Device Register(Guid organizationId, DeviceCode deviceCode, string name, string? description, DeviceSpecifications specifications,
        DeviceAssignment assignment, ExternalDeviceReference? externalReference, IEnumerable<CapabilityDefinition> capabilities)
    {
        if (organizationId == Guid.Empty)
        {
            throw new ArgumentException(
                "Organization id is required.");
        }

        ArgumentNullException.ThrowIfNull(deviceCode);
        ArgumentNullException.ThrowIfNull(specifications);
        ArgumentNullException.ThrowIfNull(assignment);
        ArgumentNullException.ThrowIfNull(capabilities);

        ValidateDetails(name, description);

        var capabilityList = capabilities.ToList();

        ValidateCapabilities(capabilityList);

        var now = DateTimeOffset.UtcNow;

        var device = new Device
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            DeviceCode = deviceCode,
            Name = name.Trim(),
            Description = Normalize(description),
            Specifications = specifications,
            Assignment = assignment,
            ExternalReference = externalReference,
            AdministrativeStatus = EDeviceAdministrativeStatus.Inactive,
            CreatedAt = now,
            UpdatedAt = now,
            Version = 1
        };

        foreach (var capability in capabilityList) {
            device._capabilities.Add(new DeviceCapability(Guid.NewGuid(), capability));
        }
        return device;
    }

    /// <summary>
    /// Updates the descriptive information of the device.
    /// </summary>
    public void UpdateDetails(string name, string? description, DeviceSpecifications specifications)
    {
        EnsureNotRetired();

        ArgumentNullException.ThrowIfNull(specifications);

        ValidateDetails(name, description);

        var normalizedName = name.Trim();
        var normalizedDescription = Normalize(description);

        if (Name == normalizedName && Description == normalizedDescription && Specifications == specifications)
        {
            return;
        }

        Name = normalizedName;
        Description = normalizedDescription;
        Specifications = specifications;

        RegisterChange();
    }

    /// <summary>
    /// Replaces the capabilities of an inactive device.
    /// Existing capabilities preserve their identifiers when the code remains the same.
    /// </summary>
    public void ReplaceCapabilities(IEnumerable<CapabilityDefinition> capabilities)
    {
        EnsureInactive();

        ArgumentNullException.ThrowIfNull(capabilities);

        var definitions = capabilities.ToList();

        ValidateCapabilities(definitions);

        if (CapabilitiesAreEquivalent(definitions))
        {
            return;
        }

        var currentCapabilities = _capabilities.ToDictionary(capability => capability.Code, StringComparer.Ordinal);

        var newCapabilities = new List<DeviceCapability>();

        foreach (var definition in definitions)
        {
            if (currentCapabilities.TryGetValue(definition.Code, out var existingCapability))
            {
                existingCapability.Update(definition);
                newCapabilities.Add(existingCapability);
            }
            else
            {
                newCapabilities.Add(new DeviceCapability(Guid.NewGuid(), definition));
            }
        }

        _capabilities.Clear();
        _capabilities.AddRange(newCapabilities);

        RegisterChange();
    }

    /// <summary>
    /// Changes the building or zone assignment of an inactive device.
    /// </summary>
    public void AssignTo(DeviceAssignment assignment)
    {
        EnsureInactive();

        ArgumentNullException.ThrowIfNull(assignment);

        if (Assignment == assignment)
        {
            return;
        }

        Assignment = assignment;

        RegisterChange();
    }

    /// <summary>
    /// Changes the administrative status of the device.
    /// </summary>
    public void ChangeAdministrativeStatus(EDeviceAdministrativeStatus newStatus)
    {
        if (AdministrativeStatus == newStatus)
        {
            return;
        }

        var validTransition =
            AdministrativeStatus == EDeviceAdministrativeStatus.Inactive &&
            newStatus == EDeviceAdministrativeStatus.Active

            ||

            AdministrativeStatus == EDeviceAdministrativeStatus.Active &&
            newStatus == EDeviceAdministrativeStatus.Inactive

            ||

            AdministrativeStatus == EDeviceAdministrativeStatus.Inactive &&
            newStatus == EDeviceAdministrativeStatus.Retired;

        if (!validTransition)
        {
            throw new InvalidOperationException($"Invalid device status transition: " + $"{AdministrativeStatus} -> {newStatus}.");
        }

        AdministrativeStatus = newStatus;

        RegisterChange();
    }

    /// <summary>
    /// Ensures the device is inactive before modifying
    /// its capabilities or assignment.
    /// </summary>
    private void EnsureInactive()
    {
        if (AdministrativeStatus != EDeviceAdministrativeStatus.Inactive)
        {
            throw new InvalidOperationException("The device must be inactive to perform this operation.");
        }
    }

    /// <summary>
    /// Prevents modifications to a retired device.
    /// </summary>
    private void EnsureNotRetired()
    {
        if (AdministrativeStatus == EDeviceAdministrativeStatus.Retired)
        {
            throw new InvalidOperationException("A retired device cannot be modified.");
        }
    }

    /// <summary>
    /// Validates the descriptive information of a device.
    /// </summary>
    private static void ValidateDetails(string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Device name is required.");
        }

        if (name.Trim().Length > 120)
        {
            throw new ArgumentException("Device name cannot exceed 120 characters.");
        }

        if (description?.Trim().Length > 500)
        {
            throw new ArgumentException("Device description cannot exceed 500 characters.");
        }
    }

    /// <summary>
    /// Validates that the device contains at least one capability
    /// and that capability codes are not duplicated.
    /// </summary>
    private static void ValidateCapabilities(IReadOnlyCollection<CapabilityDefinition> capabilities)
    {
        if (capabilities.Count == 0)
        {
            throw new ArgumentException("A device must contain at least one capability.");
        }

        var duplicatedCode = capabilities
            .GroupBy(capability => capability.Code, StringComparer.Ordinal)
            .FirstOrDefault(group => group.Count() > 1);

        if (duplicatedCode is not null)
        {
            throw new ArgumentException($"Capability '{duplicatedCode.Key}' is duplicated.");
        }
    }

    /// <summary>
    /// Determines whether the new capability collection
    /// is equivalent to the current one.
    /// </summary>
    private bool CapabilitiesAreEquivalent(IReadOnlyCollection<CapabilityDefinition> definitions)
    {
        if (_capabilities.Count != definitions.Count)
        {
            return false;
        }

        return definitions.All(definition =>
            _capabilities.Any(existing =>
                existing.Code == definition.Code &&
                existing.Kind == definition.Kind &&
                existing.Unit == definition.Unit));
    }

    /// <summary>
    /// Normalizes optional text values.
    /// </summary>
    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }

    /// <summary>
    /// Updates the modification date and aggregate version.
    /// </summary>
    private void RegisterChange()
    {
        UpdatedAt = DateTimeOffset.UtcNow;
        Version++;
    }
}