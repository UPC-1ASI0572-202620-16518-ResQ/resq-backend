namespace ResQ.API.Building_Management.Interfaces.ACL;

/// <summary>
/// Anti-Corruption Layer (ACL) interface exposed to other bounded contexts (e.g. Device Management).
/// </summary>
public interface IBuildingsContextFacade
{
    /// <summary>
    /// Validates whether a building and optional zone exist and are active for assignment.
    /// </summary>
    /// <param name="organizationId">Organization identifier.</param>
    /// <param name="buildingId">Building identifier.</param>
    /// <param name="zoneId">Optional zone identifier.</param>
    /// <returns>True if valid and active, false otherwise.</returns>
    Task<bool> ValidateAssignmentAsync(Guid organizationId, Guid buildingId, Guid? zoneId);
}
