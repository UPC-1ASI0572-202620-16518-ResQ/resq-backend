using ResQ.API.Building_Management.Domain.Model.Commands;
using ResQ.API.Building_Management.Interfaces.REST.Resources;

namespace ResQ.API.Building_Management.Interfaces.REST.Transform;

public static class UpdateBuildingDetailsCommandFromResourceAssembler
{
    public static UpdateBuildingDetailsCommand ToCommandFromResource(
        Guid organizationId, Guid buildingId,
        UpdateBuildingDetailsResource resource)
    {
        return new UpdateBuildingDetailsCommand(
            organizationId,
            buildingId,
            resource.Name,
            resource.Description,
            resource.Address?.StreetAddress,
            resource.Address?.District,
            resource.Address?.City,
            resource.Address?.CountryCode);
    }
}
