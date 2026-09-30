using ResQ.API.Building_Management.Domain.Model.Commands;
using ResQ.API.Building_Management.Domain.Model.ValueObjects;
using ResQ.API.Building_Management.Interfaces.REST.Resources;

namespace ResQ.API.Building_Management.Interfaces.REST.Transform;

public static class RegisterBuildingCommandFromResourceAssembler
{
    public static RegisterBuildingCommand ToCommandFromResource(
        Guid organizationId, RegisterBuildingResource resource)
    {
        var address = new BuildingAddress(
            resource.Address.StreetAddress,
            resource.Address.District,
            resource.Address.City,
            resource.Address.CountryCode);

        return new RegisterBuildingCommand(
            organizationId,
            resource.BuildingCode,
            resource.Name,
            resource.Description,
            address);
    }
}
