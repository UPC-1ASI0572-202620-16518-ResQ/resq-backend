using ResQ.API.Building_Management.Domain.Model.Aggregates;
using ResQ.API.Building_Management.Domain.Model.Entities;
using ResQ.API.Building_Management.Domain.Model.Queries;
using ResQ.API.Building_Management.Interfaces.REST.Resources;

namespace ResQ.API.Building_Management.Interfaces.REST.Transform;

public static class ZonePageResourceFromPageAssembler
{
    public static ZonePageResource ToResourceFromPage(
        PagedResult<Zone> page, Building building)
    {
        var items = page.Items
            .Select(z => ZoneResourceFromEntityAssembler.ToResourceFromEntity(z, building))
            .ToList();

        return new ZonePageResource(
            items, page.Page, page.Size,
            page.TotalElements, page.TotalPages);
    }
}
