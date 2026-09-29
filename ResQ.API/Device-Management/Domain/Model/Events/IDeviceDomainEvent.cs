using ResQ.API.Device_Management.Domain.Model.ValueObjects;

namespace ResQ.API.Device_Management.Domain.Model.Events;

public interface IDeviceDomainEvent
{
    DateTimeOffset OccurredAt { get; }
}

public sealed record DeviceRegistered(
    Guid DeviceId,
    DateTimeOffset OccurredAt) : IDeviceDomainEvent;

public sealed record DeviceDetailsUpdated(
    Guid DeviceId,
    DateTimeOffset OccurredAt) : IDeviceDomainEvent;

public sealed record DeviceCapabilitiesReplaced(
    Guid DeviceId,
    DateTimeOffset OccurredAt) : IDeviceDomainEvent;

public sealed record DeviceAssignedToLocation(
    Guid DeviceId,
    DeviceAssignment PreviousAssignment,
    DeviceAssignment NewAssignment,
    DateTimeOffset OccurredAt) : IDeviceDomainEvent;

public sealed record DeviceAdministrativeStatusChanged(
    Guid DeviceId,
    EDeviceAdministrativeStatus PreviousStatus,
    EDeviceAdministrativeStatus NewStatus,
    DateTimeOffset OccurredAt) : IDeviceDomainEvent;
