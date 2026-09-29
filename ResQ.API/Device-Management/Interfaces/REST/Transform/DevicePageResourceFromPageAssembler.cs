using ResQ.API.Device_Management.Domain.Model.Aggregates;
using ResQ.API.Device_Management.Domain.Model.Queries;
using ResQ.API.Device_Management.Interfaces.REST.Resources;

namespace ResQ.API.Device_Management.Interfaces.REST.Transform;

public static class DevicePageResourceFromPageAssembler
{
    public static DevicePageResource ToResourceFromPage(PagedResult<Device> page)
    {
        var items = page.Items
            .Select(DeviceResourceFromEntityAssembler.ToResourceFromEntity).ToList();

        return new DevicePageResource(
            items,
            page.Page,
            page.Size,
            page.TotalElements,
            page.TotalPages);
    }
}