using ResQ.API.Device_Management.Domain.Model.Aggregates;
using ResQ.API.Device_Management.Domain.Model.Queries;
using ResQ.API.Device_Management.Domain.Model.ValueObjects;
using ResQ.API.Shared.Domain.Repositories;

namespace ResQ.API.Device_Management.Domain.Repositories;

/// <summary>
/// Repository contract for Device aggregates.
/// </summary>
public interface IDeviceRepository : IBaseRepository<Device>
{
    Task<Device?> FindByIdAndOrganizationIdAsync(Guid deviceId, Guid organizationId);

    Task<bool> ExistsByOrganizationIdAndDeviceCodeAsync(Guid organizationId, DeviceCode deviceCode);

    Task<Device?> FindByExternalReferenceAsync(Guid organizationId, string sourceSystem, string externalDeviceId);

    Task<bool> ExistsByExternalReferenceAsync(Guid organizationId, string sourceSystem, string externalDeviceId);
    
    Task<PagedResult<Device>> FindPageAsync(Guid organizationId, Guid? buildingId, Guid? zoneId, EDeviceAdministrativeStatus? administrativeStatus, int page, int size);
}