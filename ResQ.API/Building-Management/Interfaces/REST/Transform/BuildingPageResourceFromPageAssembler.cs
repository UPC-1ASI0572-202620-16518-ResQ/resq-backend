using ResQ.API.Building_Management.Domain.Model.Aggregates;
using ResQ.API.Building_Management.Domain.Model.Queries;
using ResQ.API.Building_Management.Interfaces.REST.Resources;

namespace ResQ.API.Building_Management.Interfaces.REST.Transform;

public static class BuildingPageResourceFromPageAssembler
{
    public static BuildingPageResource ToResourceFromPage(PagedResult<Building> page)
    {
        var items = page.Items
            .Select(BuildingResourceFromEntityAssembler.ToResourceFromEntity)
            .ToList();

        return new BuildingPageResource(
            items, page.Page, page.Size,
            page.TotalElements, page.TotalPages);
    }
}
